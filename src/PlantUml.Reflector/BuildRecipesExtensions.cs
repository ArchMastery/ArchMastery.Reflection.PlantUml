using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;

namespace PlantUml.Reflector
{
    public enum WriteStrategy
    {
        OneFile, OneFilePerAssembly, OneFilePerNamespace, OneFilePerType
    }
    public static class BuildRecipesExtensions
    {

        public static List<FileInfo> WriteAll(this IEnumerable<Assembly> assemblies, DirectoryInfo directory,  WriteStrategy strategy,
            Layers layers = Layers.All, PumlDocument document = null, bool showAttributes = false)
        {
            if(!directory.Exists) directory.Create();

            List<(PumlClip clip, Layers layers)> clips = new();
            PumlWriter writer = new ();

            foreach (var assembly in assemblies)
            {
                clips.AddRange(assembly.BuildAll(layers, showAttributes));
            }

            List<FileInfo> result = new ();
            FileStream fs = null;
            switch (strategy)
            {
                case WriteStrategy.OneFile:
                    if (document is not null)
                    {
                        var oneFileFilename = Path.Combine(directory.FullName, "AllTypesDocument.puml");
                        document = document with
                        {
                            Title = document.Title ?? "All Types",
                            Clips = clips
                        };
                        fs = writer.WriteFile(oneFileFilename, document);
                    }
                    else
                    {
                        var oneFileFilename = Path.Combine(directory.FullName, "AllTypes.puml");
                        fs = writer.WriteFile(oneFileFilename, clips);
                    }
                    fs.Close();
                    result.Add(new FileInfo(fs.Name));
                    break;

                case WriteStrategy.OneFilePerType:
                    var toProcess = clips
                        .GroupBy(c => $"{c.clip.Namespace}.{c.clip.TypeName}")
                        .Select(group => (Path.Combine(directory.FullName, group.Key.AsSlug() + ".puml"), group));

                    foreach (var (filename, group) in toProcess)
                    {
                        if (document is not null)
                        {
                            var newName = filename.Split('.').ToList();
                            newName.Insert(newName.Count - 1, "Document");
                            document = document with
                            {
                                Title = document.Title ?? group.Key,
                                Clips = group
                            };
                            fs = writer.WriteFile(string.Join(".", newName), document);
                        }
                        else
                        {
                            fs = writer.WriteFile(filename, group);
                        }
                        fs.Close();
                        result.Add(new FileInfo(fs.Name));
                    }
                    break;

                case WriteStrategy.OneFilePerNamespace:
                    foreach (var group in clips.GroupBy(c => c.clip.Namespace))
                    {
                        if (document is not null)
                        {
                            var typeFilename = Path.Combine(directory.FullName, group.Key.AsSlug() + "Document.puml");
                            document = document with
                            {
                                Title = document.Title ?? group.Key,
                                Clips = clips
                            };
                            fs = writer.WriteFile(typeFilename, document);
                        }
                        else
                        {
                            var typeFilename = Path.Combine(directory.FullName, group.Key.AsSlug() + ".puml");
                            fs = writer.WriteFile(typeFilename, clips);
                        }
                        fs.Close();
                        result.Add(new FileInfo(fs.Name));
                    }
                    break;

                case WriteStrategy.OneFilePerAssembly:
                    foreach (var group in clips.GroupBy(c => c.clip.Assembly))
                    {
                        var filename = group.Key.GetName().Name!.AsSlug();
                        var typeFilename = Path.Combine(directory.FullName, $"{filename}.puml");
                        if (document is not null)
                        {
                            document = document with
                            {
                                Title = document.Title ?? group.Key.GetName().FullName,
                                Clips = clips
                            };
                            fs = writer.WriteFile(typeFilename, document);
                        }
                        else
                        {
                            fs = writer.WriteFile(typeFilename, clips);
                        }
                        fs.Close();
                        result.Add(new FileInfo(fs.Name));
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
