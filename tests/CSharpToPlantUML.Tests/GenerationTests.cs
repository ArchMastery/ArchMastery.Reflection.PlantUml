using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CSharpToPlantUML.Tests.Annotations;
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

        private GenerationTests(ITestOutputHelper output, LoggingConfig? config = null) : base(output, config)
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
    }

    public class TestClass : INotifyPropertyChanged
    {
        private string _property;

        public string Property
        {
            get => _property;
            private set => _property = value;
        }

        public TestClass(string value)
        {
            Property = value;
        }

        private void ResetProperty() => Property = null;

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
