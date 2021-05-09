using System;
using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

#nullable enable
namespace CSharpToPlantUML.Tests
{
    public class GenerationTests : LoggingTestsBase
    {
        public GenerationTests(ITestOutputHelper output) : base(output, LogLevel.Debug)
        {
        }

        [Theory]
        [InlineData(typeof(int?))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(IServiceProvider))]
        [InlineData(typeof(Environment.SpecialFolder))]
        [InlineData(typeof(byte[]))]
        public void BuiltInTypes(Type type)
        {
            TypeHolder typeHolder = new(type);

            Assert.NotNull(typeHolder);
            var clip = typeHolder.Generate(Layers.Type);
            Assert.NotNull(clip);
            Assert.NotEmpty(clip.ToString().ToCharArray());

            Output.WriteLine($"@startuml\n{clip}\n@enduml");
        }

        [Theory]
        [InlineData(typeof(StringComparer))]
        [InlineData(typeof(string))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(GenerationTests))]
        public void Inheritance(Type type)
        {
            TypeHolder typeHolder = new(type);

            Assert.NotNull(typeHolder);
            var clip = typeHolder.Generate(Layers.Inheritance);
            Assert.NotNull(clip);
            Assert.NotEmpty(clip.ToString().ToCharArray());

            Output.WriteLine($"@startuml\n{clip}\n@enduml");
        }

        [Theory]
        [InlineData(typeof(TestClass<Guid>))]
        public void NonPublicMembers(Type type)
        {
            TypeHolder typeHolder = new(type);

            Assert.NotNull(typeHolder);
            var clip = typeHolder.Generate((Layers)(Layers.TypeEnd - Layers.Public));
            Assert.NotNull(clip);
            var result = clip.ToString();
            Assert.NotEmpty(result);

            Output.WriteLine($"@startuml\n{clip}\n@enduml");
        }

        [Theory]
        [InlineData(typeof(TestClass<Guid>))]
        public void PublicMembers(Type type)
        {
            TypeHolder typeHolder = new(type);

            Assert.NotNull(typeHolder);
            var clip = typeHolder.Generate((Layers)(Layers.TypeEnd - Layers.NonPublic));
            Assert.NotNull(clip);
            var result = clip.ToString();
            Assert.NotEmpty(result);
            Output.WriteLine($"@startuml\n{result}\n@enduml");
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public void All(params Type[] types)
        {
            Output.WriteLine($"@startuml");
            foreach (var type in types)
            {
                TypeHolder typeHolder = new(type);

                Assert.NotNull(typeHolder);
                var clip = typeHolder.Generate(Layers.TypeEnd);
                Assert.NotNull(clip);
                var result = clip.ToString();
                Assert.NotEmpty(result);
                Output.WriteLine($"{result}\n");
            }
            foreach (var type in types)
            {
                TypeHolder typeHolder = new(type);

                Assert.NotNull(typeHolder);
                var clip = typeHolder.Generate(Layers.Relationships | Layers.Inheritance);
                Assert.NotNull(clip);
                var result = clip.ToString();
                Assert.NotEmpty(result);
                Output.WriteLine($"{result}\n");
            }
            Output.WriteLine($"@enduml");
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public void AllWithAttributes(params Type[] types)
        {
            Output.WriteLine($"@startuml");
            foreach (var type in types)
            {
                TypeHolder typeHolder = new(type);

                Assert.NotNull(typeHolder);
                var clip = typeHolder.Generate(Layers.TypeEnd, true);
                Assert.NotNull(clip);
                var result = clip.ToString();
                Assert.NotEmpty(result);
                Output.WriteLine($"{result}\n");
            }
            foreach (var type in types)
            {
                TypeHolder typeHolder = new(type);

                Assert.NotNull(typeHolder);
                var clip = typeHolder.Generate(Layers.Relationships | Layers.Inheritance, true);
                Assert.NotNull(clip);
                var result = clip.ToString();
                Assert.NotEmpty(result);
                Output.WriteLine($"{result}\n");
            }
            Output.WriteLine($"@enduml");
        }
    }
}
