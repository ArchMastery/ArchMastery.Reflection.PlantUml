using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PlantUml.Reflector
{
    public enum WriteStrategy
    {
        OneFile, OneFilePerAssembly, OneFilePerNamespace, OneFilePerType
    }
    public static class BuildRecipesExtensions
    {

        public static List<FileInfo> WriteAll(this IEnumerable<Assembly> assemblies, DirectoryInfo directory,  WriteStrategy strategy,
            Layers layers = Layers.All, bool showAttributes = false)
        {
            if(!directory.Exists) directory.Create();

            List<(PumlClip clip, Layers layers)> clips = new();
            PumlWriter writer = new ();

            foreach (var assembly in assemblies)
            {
                clips.AddRange(assembly.BuildAll(layers, showAttributes));
            }

            List<FileInfo> result = new ();
            switch (strategy)
            {
                case WriteStrategy.OneFile:
                    var oneFileFilename = Path.Combine(directory.FullName, "AllTypes.puml");
                    var oneFileFs = writer.WriteFile(oneFileFilename, clips);
                    oneFileFs.Close();
                    result.Add(new FileInfo(oneFileFs.Name));
                    break;

                case WriteStrategy.OneFilePerType:
                    foreach (var typeFs in clips
                        .GroupBy(c => $"{c.clip.Namespace}.{c.clip.TypeName}")
                        .Select(group => (Path.Combine(directory.FullName, group.Key.AsSlug() + ".puml"), group))
                        .Select(pair => writer.WriteFile(pair.Item1, pair.group)))
                    {
                        typeFs.Close();
                        result.Add(new FileInfo(typeFs.Name));
                    }
                    break;

                case WriteStrategy.OneFilePerNamespace:
                    foreach (var group in clips.GroupBy(c => c.clip.Namespace))
                    {
                        var typeFilename = Path.Combine(directory.FullName, group.Key.AsSlug() + ".puml");
                        var typeFs = writer.WriteFile(typeFilename, group);
                        typeFs.Close();
                        result.Add(new FileInfo(typeFs.Name));
                    }
                    break;

                case WriteStrategy.OneFilePerAssembly:
                    foreach (var group in clips.GroupBy(c => c.clip.Assembly))
                    {
                        var filename = group.Key.GetName().Name!.AsSlug();
                        var typeFilename = Path.Combine(directory.FullName, $"{filename}.puml");
                        var typeFs = writer.WriteFile(typeFilename, group);
                        typeFs.Close();
                        result.Add(new FileInfo(typeFs.Name));
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }

            return result;
        }

        public static IEnumerable<(PumlClip clip, Layers layers)> BuildAll(this Assembly assembly,
            Layers layers = Layers.All, bool showAttributes = false)
        {
            var types = assembly.GetTypes().Where(t => t.ToString() != "<PrivateImplementationDetails>");
            types = types.Where(t => t.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) is null);

            return BuildTypes(types, layers, showAttributes);
        }

        private static IEnumerable<(PumlClip clip, Layers layers)> BuildTypes(IEnumerable<Type> typesEnumerable, 
            Layers layers = Layers.All, bool showAttributes = false)
        {
            var enumerable = typesEnumerable.ToArray();
            List<(PumlClip clip, Layers layers)> result = new ();

            var layer = layers;

            if (layers > Layers.TypeEnd)
            {
                layer = Layers.TypeEnd;
            }

            while (layers != Layers.None)
            {
                result.AddRange(
                    from type in enumerable 
                    select new TypeHolder(type) into holder 
                    select holder.Generate(layers, showAttributes) into clip 
                    select (clip, layer));

                if (layers > Layers.TypeEnd)
                {
                    layer = layers;
                    layers = layers switch
                    {
                        Layers.Relationships => Layers.Relationships,
                        Layers.Inheritance => Layers.Inheritance,
                        Layers.All => Layers.Inheritance | Layers.Relationships | Layers.Notes,
                        _ => Layers.None
                    };
                }
                else
                {
                    layers = Layers.None;
                }
            }

            return result;
        }
    }
}
