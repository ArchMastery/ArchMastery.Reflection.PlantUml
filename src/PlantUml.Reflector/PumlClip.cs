using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace PlantUml.Reflector
{
    public class PumlClip
    {
        private int _version = -1;
        private int _rendered = -1;
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
        public int Version => _version;

        public string ToString(Layers layers)
        {
            if (_rendered > -1 && _version > -1 && _rendered == _version) return _cached;

            var sb = new StringBuilder();
            lock (_padlock)
            {
                _cached = null;

                var toRender = (layers switch
                {
                    Layers.All => Segments.GroupBy(tuple => tuple.Item1, tuple => tuple.Item2),
                    _ => Segments.Where(tuple => tuple.Item1 <= layers)
                        .GroupBy(tuple => tuple.Item1, tuple => tuple.Item2)

                }).OrderBy(g => (int)g.Key);

                foreach (var item in toRender)
                {
                    item.ToList().ForEach(value => sb.AppendLine(value));
                }

                _rendered = _version;
                _cached = sb.ToString();
            }

            return sb.ToString();
        }

        private static readonly object _padlock = new ();

        public override string ToString()
        {
            return ToString(Layers.All);
        }
    }
}