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

            Output.WriteLine(clip.ToString());
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

            Output.WriteLine(clip.ToString());
        }

        [Theory]
        [InlineData(typeof(TestClass))]
        public void NonPublicMembers(Type type)
        {
            TypeHolder typeHolder = new(type);

            Assert.NotNull(typeHolder);
            var clip = typeHolder.Generate(Layers.NonPublic);
            Assert.NotNull(clip);
            var result = clip.ToString();
            Assert.NotEmpty(result);

            Output.WriteLine(clip.ToString());
        }

        [Theory]
        [InlineData(typeof(TestClass))]
        public void PublicMembers(Type type)
        {
            TypeHolder typeHolder = new(type);

            Assert.NotNull(typeHolder);
            var clip = typeHolder.Generate(Layers.Members);
            Assert.NotNull(clip);
            var result = clip.ToString();
            Assert.NotEmpty(result);

            Output.WriteLine(clip.ToString());
        }
    }
}
