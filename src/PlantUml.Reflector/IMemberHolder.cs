#nullable enable

namespace PlantUml.Reflector
{
    public interface IMemberHolder
    {
        (Layers layers, string segment) Segment { get; set; }
    }

    public class MemberHolder<TInfo> : IMemberHolder
        where TInfo : class
    {
        public TInfo Info { get; }
        public (Layers layers, string segment) Segment { get; set; }

        public MemberHolder(TInfo info, (Layers, string) segment)
        {
            Info = info;
            Segment = segment;
        }

        public string ToString(Layers layers) =>
            layers >= Segment.layers
                ? Segment.segment
                : string.Empty;
    }
}