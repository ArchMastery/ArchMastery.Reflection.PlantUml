using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace CSharpToPlantUML
{
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
}