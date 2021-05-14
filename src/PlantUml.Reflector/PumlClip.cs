using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PlantUml.Reflector
{
    public class PumlClip
    {
        private int _version = -1;
        private int _rendered = -1;
        private string _cached;

        public PumlClip(string typeName, string @namespace, Assembly assembly)
        {
            TypeName = typeName;
            Namespace = @namespace;
            Assembly = assembly;
            Segments.CollectionChanged += SegmentsOnCollectionChanged;
        }

        private void SegmentsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _version++;
        }

        public ObservableCollection<IMemberHolder> Segments { get; set; } = new ();
        public int Version => _version;
        public string TypeName { get; }
        public string Namespace { get; }
        public Assembly Assembly { get; }

        public string ToString(Layers layers)
        {
            if (_rendered > -1 && _version > -1 && _rendered == _version) return _cached;

            var sb = new StringBuilder();
            lock (_padlock)
            {
                _cached = null;

                var toRender = (layers switch
                {
                    Layers.All => Segments.GroupBy(member => member.Segment.layers, member => member.Segment.segment),
                    _ => Segments.Where(member => member.Segment.layers <= layers)
                        .GroupBy(member => member.Segment.layers, member => member.Segment.segment)

                }).OrderBy(g => (int)g.Key);

                foreach (var item in toRender)
                {
                    item.ToList().ForEach(value => {
                        if(!string.IsNullOrWhiteSpace(value)) 
                        {
                            sb.AppendLine(value);
                        }
                    });
                }

                _rendered = _version;
                _cached = Environment.NewLine + sb.ToString().Trim();
            }

            return _cached;
        }

        private static readonly object _padlock = new ();

        public override string ToString()
        {
            return ToString(Layers.All);
        }
    }
}
