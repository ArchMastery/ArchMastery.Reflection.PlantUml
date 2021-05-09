using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CSharpToPlantUML.Tests.Annotations;

#nullable enable
namespace CSharpToPlantUML.Tests
{
    public abstract class TestBase<TValue>
        where TValue : struct
    {

    }

    public record MyEntity(string Message);

    public class TestClass<TValue> : TestBase<TValue>, INotifyPropertyChanged
        where TValue : struct
    {
        private static readonly InnerClass<TValue>[] _innerClass = null;
        private static readonly IEnumerable<InnerClass<DateTime>> _datesField = new List<InnerClass<DateTime>>();

        protected InnerClass<TValue>[] InnerProperty => _innerClass;
        internal IEnumerable<InnerClass<DateTime>> DatesProperty => _datesField;
        
        public string Property
        {
            get;
            private set;
        }

        public TestClass(string value)
        {
            Property = value;
        }

        private void ResetProperty() => Property = string.Empty;

        public TValue Convert<TFrom>(TFrom from)
            where TFrom : struct
        {
            return (TValue) (object) from;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static InnerClass<TValue> CreateInnerClass() => _innerClass.FirstOrDefault() ?? new InnerClass<TValue>();

        public class InnerClass<T>
            where T : struct
        {
            private static T _value = default;
        }

    }

    public static class Extensions
    {
        public static string GetName<TValue, T>(this TestClass<TValue>.InnerClass<T> innerClass)
            where TValue : struct
            where T : struct
        {
            return innerClass.GetType().FullName ?? innerClass.GetType().Name;
        }
    }
}
