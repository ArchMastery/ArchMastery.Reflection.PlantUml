using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace CSharpToPlantUML
{
    public class TypeHolder
    {
        public Type Type { get; }

        public TypeHolder(Type type)
        {
            Type = type;
        }

        public PumlClip Generate(Layers layers)
        {
            var result = new PumlClip();

            switch (layers)
            {
                case Layers.Type:
                    var map = (Type.IsClass, Type.IsEnum, Type.IsArray, Type.IsInterface, Type.IsValueType);
                    result.Segments.Add((layers, @$"{(map switch
                    {
                        (true, _, _, _, _) => "class",
                        _ => "component"
                    })} ""{Type.Name}"" as ""{Type.FullName}"""));
                    break;
            }
            
            return result;
        }
    }

    public class PumlClip
    {
        private int _version;
        private int _rendered;
        private string _cached;

        public PumlClip()
        {
            Segments.CollectionChanged += SegmentsOnCollectionChanged;
        }

        private void SegmentsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _version++;
        }

        public ObservableCollection<(Layers, string)> Segments { get; set; } = new ();

        public string ToString(Layers layers)
        {
            if (_rendered == _version) return _cached;

            var sb = new StringBuilder();
            lock (_padlock)
            {
                _cached = null;

                var toRender = layers switch
                {
                    Layers.All => Segments.GroupBy(tuple => tuple.Item1, tuple => tuple.Item2),
                    _ => Segments.Where(tuple => (tuple.Item1 & layers) == layers)
                        .GroupBy(tuple => tuple.Item1, tuple => tuple.Item2)
                };

                foreach (var item in toRender.OrderBy(grouping => (int) grouping.Key))
                {
                    item.ToList().ForEach(value => sb.AppendLine(value));
                }

                _rendered = _version;
                _cached = sb.ToString();
            }

            return sb.ToString();
        }

        private static object _padlock = new object();

        public override string ToString()
        {
            return ToString(Layers.All);
        }
    }

    public class Layer
    {
        public Layer(Layers layers)
        {
            Layers = layers;
        }

        public Layers Layers { get; init;  }

        public bool Shows(Layers target)
        {
            return Layers switch
            {
                Layers.All => true,
                _ => (Layers & target) == target 
            };
        }
    }

    [Flags]
    public enum Layers
    {
        Type, Public, Protected, Private, Relationships, Inheritance, Notes, All
    }
}