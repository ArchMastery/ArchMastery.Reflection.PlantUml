using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PlantUml.Reflector.xUnit.Annotations;

#nullable enable
namespace PlantUml.Reflector.xUnit
{
    public abstract class TestBase<TValue>
        where TValue : struct
    {

    }

    public record MyEntity(string Message);

    public sealed class TestClass<TValue> : TestBase<TValue>, INotifyPropertyChanged
        where TValue : struct
    {
        private static readonly InnerClass<TValue>[] _innerClass = null;
        private static readonly IEnumerable<InnerClass<DateTime>> _datesField = new List<InnerClass<DateTime>>();

        private InnerClass<TValue>[] InnerProperty => _innerClass;
        internal IEnumerable<InnerClass<DateTime>> DatesProperty => _datesField;

        private string Property
        {
            get;
            set;
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
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
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
