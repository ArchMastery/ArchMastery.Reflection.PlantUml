using System.ComponentModel;
using System.Runtime.CompilerServices;
using CSharpToPlantUML.Tests.Annotations;

#nullable enable
namespace CSharpToPlantUML.Tests
{
    public class TestClass : INotifyPropertyChanged
    {
        private string _property = string.Empty;

        public string Property
        {
            get => _property;
            private set => _property = value;
        }

        public TestClass(string value)
        {
            Property = value;
        }

        private void ResetProperty() => Property = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
