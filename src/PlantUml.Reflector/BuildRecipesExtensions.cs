using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PlantUml.Reflector
{
    public static class BuildRecipesExtensions
    {
        public static IEnumerable<(PumlClip clip, Layers layers)> BuildAll(this Assembly assembly, Layers layers = Layers.All, bool showAttributes = false)
        {
            var result = new List<(PumlClip clip, Layers layers)>();

            var types = assembly.GetTypes().Where(t => t.ToString() != "<PrivateImplementationDetails>");

            var layer = layers;

            if (layers > Layers.TypeEnd)
            {
                layer = Layers.TypeEnd;
            }

            while (layer != Layers.None)
            {
                foreach (var type in types)
                {
                    var holder = new TypeHolder(type);
                    var clip = holder.Generate(layers, showAttributes);
                    result.Add((clip, layer));
                }

                if (layers > Layers.TypeEnd)
                {
                    if (layers == Layers.TypeEnd)
                    {
                        layer = layers switch
                                {
                                    Layers.Relationships => Layers.Relationships,
                                    Layers.Inheritance => Layers.Inheritance,
                                    Layers.All => Layers.Inheritance | Layers.Relationships | Layers.Notes,
                                    _ => Layers.None
                                };
                    }
                    else
                    {
                        layer = Layers.None;
                    }
                }
                else
                {
                    layer = Layers.None;
                }
            }

            return result;
        }
    }
}
