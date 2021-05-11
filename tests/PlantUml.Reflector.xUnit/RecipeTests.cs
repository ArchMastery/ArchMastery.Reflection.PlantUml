using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Divergic.Logging.Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit;

namespace PlantUml.Reflector.xUnit
{
    public class RecipeTests : LoggingTestsBase
    {
        private const string AssemblyPath = "./TestAssemblies/TestAssembly.dll";
        public RecipeTests(ITestOutputHelper output) : base(output, LogLevel.Debug)
        {
        }


        private PumlDocument _document = new ()
        {
            Direction = PumlDirection.LeftToRight,
            LineMode = PumlLineMode.Orthogonal,
            FooterNote = "From Test"
        };

        [Theory]
        [InlineData(AssemblyPath, Layers.All, true)]
        public void WriteDocumentPerType(string assemblyPath, Layers layers, bool includeAttributes)
        {
            if (OperatingSystem.IsWindows()) assemblyPath = assemblyPath.Replace("/", "\\");
            var path = Path.Combine(Environment.CurrentDirectory, assemblyPath);
            var assembly = Assembly.LoadFile(path);

            assembly.Should().NotBeNull();

            var directoryInfo = new DirectoryInfo($"{Path.GetFileNameWithoutExtension(path)}\\PerType");

            if (directoryInfo.Exists && directoryInfo.GetFiles().Length > 0)
            {
                directoryInfo.GetFiles().Select(f => f.FullName).ToList().ForEach(File.Delete);
            }

            var types = assembly.GetTypes().Where(t => t.ToString() != "<PrivateImplementationDetails>");
            types = types.Where(t => t.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) is null);

            var result = new[] { assembly }.WriteAll(
                directoryInfo,
                WriteStrategy.OneFilePerType,
                layers,
                _document,
                includeAttributes);

            result.Should().NotBeNullOrEmpty();
            result.Count.Should().Be(types.Count());

            var file = result.First();

            file.Should().NotBeNull();
            File.Exists(file.FullName).Should().BeTrue();
            Output.WriteLine(file.FullName);
        }
        [Theory]
        [InlineData(AssemblyPath, Layers.All, true)]
        public void WriteDocumentPerNamespace(string assemblyPath, Layers layers, bool includeAttributes)
        {
            if (OperatingSystem.IsWindows()) assemblyPath = assemblyPath.Replace("/", "\\");
            var path = Path.Combine(Environment.CurrentDirectory, assemblyPath);
            var assembly = Assembly.LoadFile(path);

            assembly.Should().NotBeNull();

            var directoryInfo = new DirectoryInfo($"{Path.GetFileNameWithoutExtension(path)}\\PerNamespace");

            if (directoryInfo.Exists && directoryInfo.GetFiles().Length > 0)
            {
                directoryInfo.GetFiles().Select(f => f.FullName).ToList().ForEach(File.Delete);
            }

            var types = assembly.GetTypes().Where(t => t.ToString() != "<PrivateImplementationDetails>");
            types = types.Where(t => t.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) is null);
            var namespaces = types.GroupBy(t => t.Namespace);

            var result = new[] { assembly }.WriteAll(
                directoryInfo,
                WriteStrategy.OneFilePerNamespace,
                layers,
                _document,
                includeAttributes);

            result.Should().NotBeNullOrEmpty();
            result.Count.Should().Be(namespaces.Count());

            var file = result.First();

            file.Should().NotBeNull();
            File.Exists(file.FullName).Should().BeTrue();
            Output.WriteLine(file.FullName);
        }
        [Theory]
        [InlineData(AssemblyPath, Layers.All, true)]
        public void WriteDocumentPerAssembly(string assemblyPath, Layers layers, bool includeAttributes)
        {
            if (OperatingSystem.IsWindows()) assemblyPath = assemblyPath.Replace("/", "\\");
            var path = Path.Combine(Environment.CurrentDirectory, assemblyPath);
            var assembly = Assembly.LoadFile(path);

            assembly.Should().NotBeNull();

            var directoryInfo = new DirectoryInfo($"{Path.GetFileNameWithoutExtension(path)}\\PerAssembly");

            if (directoryInfo.Exists && directoryInfo.GetFiles().Length > 0)
            {
                directoryInfo.GetFiles().Select(f => f.FullName).ToList().ForEach(File.Delete);
            }

            var result = new[] { assembly }.WriteAll(
                directoryInfo,
                WriteStrategy.OneFilePerAssembly,
                layers,
                _document,
                includeAttributes);

            result.Should().NotBeNullOrEmpty();
            result.Count.Should().Be(1);

            var file = result.First();

            file.Should().NotBeNull();
            File.Exists(file.FullName).Should().BeTrue();
            Output.WriteLine(file.FullName);
        }

        [Theory]
        [InlineData(AssemblyPath, Layers.All, true)]
        public void WriteFilePerType(string assemblyPath, Layers layers, bool includeAttributes)
        {
            if (OperatingSystem.IsWindows()) assemblyPath = assemblyPath.Replace("/", "\\");
            var path = Path.Combine(Environment.CurrentDirectory, assemblyPath);
            var assembly = Assembly.LoadFile(path);

            assembly.Should().NotBeNull();

            var directoryInfo = new DirectoryInfo($"{Path.GetFileNameWithoutExtension(path)}\\PerType");

            if (directoryInfo.Exists && directoryInfo.GetFiles().Length > 0)
            {
                directoryInfo.GetFiles().Select(f => f.FullName).ToList().ForEach(File.Delete);
            }

            var types = assembly.GetTypes().Where(t => t.ToString() != "<PrivateImplementationDetails>");
            types = types.Where(t => t.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) is null);

            var result = new[] { assembly }.WriteAll(
                directoryInfo,
                WriteStrategy.OneFilePerType,
                layers,
                null,
                includeAttributes);

            result.Should().NotBeNullOrEmpty();
            result.Count.Should().Be(types.Count());

            var file = result.First();

            file.Should().NotBeNull();
            File.Exists(file.FullName).Should().BeTrue();
            Output.WriteLine(file.FullName);
        }
        [Theory]
        [InlineData(AssemblyPath, Layers.All, true)]
        public void WriteFilePerNamespace(string assemblyPath, Layers layers, bool includeAttributes)
        {
            if (OperatingSystem.IsWindows()) assemblyPath = assemblyPath.Replace("/", "\\");
            var path = Path.Combine(Environment.CurrentDirectory, assemblyPath);
            var assembly = Assembly.LoadFile(path);

            assembly.Should().NotBeNull();

            var directoryInfo = new DirectoryInfo($"{Path.GetFileNameWithoutExtension(path)}\\PerNamespace");

            if (directoryInfo.Exists && directoryInfo.GetFiles().Length > 0)
            {
                directoryInfo.GetFiles().Select(f => f.FullName).ToList().ForEach(File.Delete);
            }

            var types = assembly.GetTypes().Where(t => t.ToString() != "<PrivateImplementationDetails>");
            types = types.Where(t => t.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) is null);
            var namespaces = types.GroupBy(t => t.Namespace);

            var result = new[] { assembly }.WriteAll(
                directoryInfo,
                WriteStrategy.OneFilePerNamespace,
                layers,
                null,
                includeAttributes);

            result.Should().NotBeNullOrEmpty();
            result.Count.Should().Be(namespaces.Count());

            var file = result.First();

            file.Should().NotBeNull();
            File.Exists(file.FullName).Should().BeTrue();
            Output.WriteLine(file.FullName);
        }
        [Theory]
        [InlineData(AssemblyPath, Layers.All, true)]
        public void WriteFilePerAssembly(string assemblyPath, Layers layers, bool includeAttributes)
        {
            if (OperatingSystem.IsWindows()) assemblyPath = assemblyPath.Replace("/", "\\");
            var path = Path.Combine(Environment.CurrentDirectory, assemblyPath);
            var assembly = Assembly.LoadFile(path);

            assembly.Should().NotBeNull();

            var directoryInfo = new DirectoryInfo($"{Path.GetFileNameWithoutExtension(path)}\\PerAssembly");

            if (directoryInfo.Exists && directoryInfo.GetFiles().Length > 0)
            {
                directoryInfo.GetFiles().Select(f => f.FullName).ToList().ForEach(File.Delete);
            }

            var result = new[] { assembly }.WriteAll(
                directoryInfo,
                WriteStrategy.OneFilePerAssembly,
                layers,
                null,
                includeAttributes);

            result.Should().NotBeNullOrEmpty();
            result.Count.Should().Be(1);

            var file = result.First();

            file.Should().NotBeNull();
            File.Exists(file.FullName).Should().BeTrue();
            Output.WriteLine(file.FullName);
        }

        [Theory]
        [InlineData(AssemblyPath, Layers.All, true)]
        public void AllInAssembly(string assemblyPath, Layers layers, bool includeAttributes)
        {
            var path = Path.Combine(Environment.CurrentDirectory, assemblyPath);
            var assembly = Assembly.LoadFile(path);

            assembly.Should().NotBeNull();

            var results = assembly.BuildAll(layers, includeAttributes).ToList();

            results.Should().NotBeNullOrEmpty();

            var types = assembly.GetTypes().Where(t => t.ToString() != "<PrivateImplementationDetails>").ToList();
            types = types.Where(t => t.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) is null).ToList();

            var resultCount = results.GroupBy(i => i.clip.Namespace + "." + i.clip.TypeName).Count();

            Output.WriteLine($"types: {types.Count}");
            Output.WriteLine($"results.Count: {resultCount}");

            resultCount.Should().Be(types.Count);

            results.GroupBy(r => r.layers).Count().Should().Be(2);

            var holder = new TypeHolder(GetType());
            var counter = 0;

            foreach (var type in types)
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

                var name = objectType + holder.NormalizeName(type)?.AsSlug() + " ";

                try
                {
                    var (clip, _) = results.SingleOrDefault(pair => pair.clip.ToString(pair.layers)
                                                                            .Contains(name));

                    clip.Should().NotBeNull($"name: {name} not found in any clip.\n`{type} : {holder.NormalizeName(type.BaseType)}`");

                    ++counter;
                }
                catch (InvalidOperationException ex) when (ex.Message == "Sequence contains more than one matching element")
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

                
            }

            counter.Should().Be(types.Count);

            Output.WriteLine("@startuml");
            results.ToList().ForEach(r => Output.WriteLine(r.clip.ToString(r.layers)));
            Output.WriteLine("@enduml");
        }
    }
}
