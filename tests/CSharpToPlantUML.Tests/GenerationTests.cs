using System;
using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

#nullable enable
namespace CSharpToPlantUML.Tests
{
    public class UnitTest1 : LoggingTestsBase
    {
        public UnitTest1(ITestOutputHelper output) : base(output, LogLevel.Debug)
        {
        }

        private UnitTest1(ITestOutputHelper output, LoggingConfig? config = null) : base(output, config)
        {
        }
        
        [Theory]
        [InlineData(typeof(String))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(IServiceProvider))]
        [InlineData(typeof(Environment.SpecialFolder))]
        [InlineData(typeof(byte[]))]
        public void BuiltInTypes(Type type)
        {
            TypeHolder typeHolder = new(type);

            Assert.NotNull(typeHolder);
            var clip = typeHolder.Generate(Layers.All);
            Assert.NotNull(clip);
            Assert.NotEmpty(clip.ToString().ToCharArray());
            
            Output.WriteLine(clip.ToString());
        }      
        
        [Theory]
        [InlineData(typeof(StringComparer))]
        public void Inheritance(Type type)
        {
            TypeHolder typeHolder = new(type);

            Assert.NotNull(typeHolder);
            var clip = typeHolder.Generate(Layers.All);
            Assert.NotNull(clip);
            Assert.NotEmpty(clip.ToString().ToCharArray());
            
            Output.WriteLine(clip.ToString());
        }
    }
}
