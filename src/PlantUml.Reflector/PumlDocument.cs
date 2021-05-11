using System.Collections.Generic;

namespace PlantUml.Reflector
{
    public record PumlDocument
    {
        public string Title { get; init; }

        public PumlDirection Direction { get; init; } = PumlDirection.TopToBottom;

        public PumlLineMode LineMode { get; init; } = PumlLineMode.Default;

        public string Styles { get; init; }

        public string HeaderComment { get; init; }
        public string FooterNote { get; init; }

        public IEnumerable<(PumlClip clip, Layers layer)> Clips { get; init; } =
            System.Array.Empty<(PumlClip clip, Layers layer)>();
    }
}
