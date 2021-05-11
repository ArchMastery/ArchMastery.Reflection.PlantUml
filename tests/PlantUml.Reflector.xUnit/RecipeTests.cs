using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Divergic.Logging.Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using PlantUml.Reflector;
using Xunit;

namespace PlantUml.Reflector.xUnit
{
    public class RecipeTests : LoggingTestsBase
    {
        private const string AssemblyPath = "./TestAssemblies/TestAssembly.dll";
        public RecipeTests(ITestOutputHelper output) : base(output, LogLevel.Debug)
        {
        }

        [Theory]
        [InlineData(AssemblyPath, Layers.All, true)]
        public void AllInAssembly(string assemblyPath, Layers layers, bool includeAttributes)
        {
            var path = Path.Combine(Environment.CurrentDirectory, assemblyPath);
            var assembly = Assembly.LoadFile(path);

            assembly.Should().NotBeNull();

            var results = assembly.BuildAll(layers, includeAttributes);

            results.Should().NotBeNullOrEmpty();

            Output.WriteLine($"types: {assembly.GetTypes().Length}");
            Output.WriteLine($"results.Count: {results.Count()}");

            results.Count().Should().Equals(assembly.GetTypes().Length);

            results.GroupBy(r => r.layers).Count().Should().Be(1);

            var holder = new TypeHolder(GetType());
            var counter = 0;
            foreach (var type in assembly.GetTypes().Where(t => t.ToString() != "<PrivateImplementationDetails>"))
            {
                var typeMap =
                    (type.IsClass && !type.IsAbstract,
                     type.IsClass && type.IsAbstract, 
                     type.IsEnum, type.IsArray, type.IsInterface,
                     type.IsValueType);

                var objectType = typeMap switch
                                 {
                                     (true, _, _, _, _, _) => "class ",
                                     (_, true, _, _, _, _) => "abstract class ",
                                     (_, _, true, _, _, _) => "enum ",
                                     (_, _, _, _, true, _) => "interface ",
                                     _ => "entity "
                                 };

                var name = objectType + holder.NormalizeName(type).AsSlug() + " ";

                try
                {
                    var (clip, layer) = results.SingleOrDefault(pair => pair.clip.ToString(pair.layers)
                                                                            .Contains(name));

                    clip.Should().NotBeNull($"name: {name} not found in any clip.\n`{type} : {holder.NormalizeName(type.BaseType)}`");
                }
                catch (System.InvalidOperationException ex) when (ex.Message == "Sequence contains more than one matching element")
                {
                    var set = results.Where(pair => pair.clip.ToString(pair.layers).Contains(name)).ToList();

                    set.ForEach(pair =>
                                {
                                    var (clip, layer) = pair;
                                    Output.WriteLine(new string('-', 80));
                                    Output.WriteLine(clip.ToString(layer));
                                });

                    set.Count.Should().Be(1, $"`{name}` exists multiple times.");
                }

                ++counter;
            }

            counter.Should().Be(results.Count());

            Output.WriteLine("@startuml");
            results.ToList().ForEach(r => Output.WriteLine(r.clip.ToString(r.layers)));
            Output.WriteLine("@enduml");
        }
    }
}
