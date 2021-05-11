using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PlantUml.Reflector
{
    public class PumlWriter
    {
        public Encoding Encoding { get; } = Encoding.UTF8;

        public FileStream WriteFile(string path, IEnumerable<(PumlClip clip, Layers layer)> clips)
        {
            var stream = new FileStream(path, FileMode.Append | FileMode.OpenOrCreate);

            stream.Write(Encoding.GetBytes("@startuml\n"));

            foreach (var (clip, layers) in clips)
            {
                var puml = clip.ToString(layers);

                stream.Write(Encoding.GetBytes(puml));
            }

            stream.Write(Encoding.GetBytes("\n@enduml"));

            stream.Flush();

            return stream;
        }

        public async Task<FileStream> WriteFileAsync(string path, IEnumerable<(PumlClip clip, Layers layer)> clips)
        {
            var stream = new FileStream(path, FileMode.Append | FileMode.OpenOrCreate);

            await stream.WriteAsync(Encoding.GetBytes("@startuml\n"));

            foreach (var (clip, layers) in clips)
            {
                var puml = clip.ToString(layers);

                await stream.WriteAsync(Encoding.GetBytes(puml));
            }

            await stream.WriteAsync(Encoding.GetBytes("\n@enduml"));

            await stream.FlushAsync();

            return stream;
        }

        public Stream WriteStream(Stream stream, IEnumerable<(PumlClip clip, Layers layer)> clips)
        {
            stream.Write(Encoding.GetBytes("@startuml\n"));

            foreach (var (clip, layers) in clips)
            {
                var puml = clip.ToString(layers);

                stream.Write(Encoding.GetBytes(puml));
            }

            stream.Write(Encoding.GetBytes("\n@enduml"));

            stream.Flush();

            return stream;
        }

        public async Task<Stream> WriteStreamAsync(Stream stream,IEnumerable<(PumlClip clip, Layers layer)> clips)
        {
            await stream.WriteAsync(Encoding.GetBytes("@startuml\n"));

            foreach (var (clip, layers) in clips)
            {
                var puml = clip.ToString(layers);

                await stream.WriteAsync(Encoding.GetBytes(puml));
            }

            await stream.WriteAsync(Encoding.GetBytes("\n@enduml"));

            await stream.FlushAsync();

            return stream;
        }

        public FileStream WriteFile(string path, PumlDocument document)
        {
            var stream = new FileStream(path, FileMode.Append | FileMode.OpenOrCreate);

            if(!string.IsNullOrWhiteSpace(document.HeaderComment)) stream.Write(Encoding.GetBytes("' " + document.HeaderComment.Replace("\n", "\n' ") + "\n"));
            stream.Write(Encoding.GetBytes($"@startuml {Path.GetFileNameWithoutExtension(path)}\n"));
            if(!string.IsNullOrWhiteSpace(document.Title)) stream.Write(Encoding.GetBytes($"title {document.Title}\n"));
            stream.Write(Encoding.GetBytes($"{(document.Direction != PumlDirection.TopToBottom ? "left to right direction\n" : string.Empty)}"));
            stream.Write(Encoding.GetBytes($"{document.LineMode switch { PumlLineMode.Orthogonal => "skinparam linetype ortho\n", PumlLineMode.Polyline => "skinparam linetype polyline\n" }}"));
            if(!string.IsNullOrWhiteSpace(document.Styles)) stream.Write(Encoding.GetBytes($"{document.Styles}\n"));

            foreach (var (clip, layers) in document.Clips)
            {
                var puml = clip.ToString(layers);

                stream.Write(Encoding.GetBytes(puml));
            }

            if(!string.IsNullOrWhiteSpace(document.FooterNote)) stream.Write(Encoding.GetBytes($"\nnote as footer\n\t{document.FooterNote}\nend note"));
            stream.Write(Encoding.GetBytes("\n@enduml"));

            stream.Flush();

            return stream;
        }
        public async Task<FileStream> WriteFileAsync(string path, PumlDocument document)
        {
            var stream = new FileStream(path, FileMode.Append | FileMode.OpenOrCreate);

            if(!string.IsNullOrWhiteSpace(document.HeaderComment)) await stream.WriteAsync(Encoding.GetBytes("' " + document.HeaderComment.Replace("\n", "\n' ") + "\n"));
            await stream.WriteAsync(Encoding.GetBytes($"@startuml {Path.GetFileNameWithoutExtension(path)}\n"));
            if(!string.IsNullOrWhiteSpace(document.Title)) await stream.WriteAsync(Encoding.GetBytes($"title {document.Title}\n"));
            await stream.WriteAsync(Encoding.GetBytes($"{(document.Direction != PumlDirection.TopToBottom ? "left to right direction\n" : string.Empty)}"));
            await stream.WriteAsync(Encoding.GetBytes($"{document.LineMode switch { PumlLineMode.Orthogonal => "skinparam linetype ortho\n", PumlLineMode.Polyline => "skinparam linetype polyline\n" }}"));
            if(!string.IsNullOrWhiteSpace(document.Styles)) await stream.WriteAsync(Encoding.GetBytes($"{document.Styles}\n"));

            foreach (var (clip, layers) in document.Clips)
            {
                var puml = clip.ToString(layers);

                await stream.WriteAsync(Encoding.GetBytes(puml));
            }

            if(!string.IsNullOrWhiteSpace(document.FooterNote)) await stream.WriteAsync(Encoding.GetBytes($"\nnote as footer\n\t{document.FooterNote}\nend note"));
            await stream.WriteAsync(Encoding.GetBytes("\n@enduml"));

            await stream.FlushAsync();

            return stream;
        }
        public Stream WriteStream(Stream stream, PumlDocument document)
        {
            if(!string.IsNullOrWhiteSpace(document.HeaderComment)) stream.Write(Encoding.GetBytes("' " + document.HeaderComment.Replace("\n", "\n' ") + "\n"));
            stream.Write(Encoding.GetBytes($"@startuml\n"));
            if(!string.IsNullOrWhiteSpace(document.Title)) stream.Write(Encoding.GetBytes($"title {document.Title}\n"));
            stream.Write(Encoding.GetBytes($"{(document.Direction != PumlDirection.TopToBottom ? "left to right direction\n" : string.Empty)}"));
            stream.Write(Encoding.GetBytes($"{document.LineMode switch { PumlLineMode.Orthogonal => "skinparam linetype ortho\n", PumlLineMode.Polyline => "skinparam linetype polyline\n" }}"));
            if(!string.IsNullOrWhiteSpace(document.Styles)) stream.Write(Encoding.GetBytes($"{document.Styles}\n"));

            foreach (var (clip, layers) in document.Clips)
            {
                var puml = clip.ToString(layers);

                stream.Write(Encoding.GetBytes(puml));
            }

            if(!string.IsNullOrWhiteSpace(document.FooterNote)) stream.Write(Encoding.GetBytes($"\nnote as footer\n\t{document.FooterNote}\nend note"));
            stream.Write(Encoding.GetBytes("\n@enduml"));

            stream.Flush();

            return stream;
        }
        public async Task<Stream> WriteStreamAsync(Stream stream, PumlDocument document)
        {
            if(!string.IsNullOrWhiteSpace(document.HeaderComment)) await stream.WriteAsync(Encoding.GetBytes("' " + document.HeaderComment.Replace("\n", "\n' ") + "\n"));
            await stream.WriteAsync(Encoding.GetBytes($"@startuml\n"));
            if(!string.IsNullOrWhiteSpace(document.Title)) await stream.WriteAsync(Encoding.GetBytes($"title {document.Title}\n"));
            await stream.WriteAsync(Encoding.GetBytes($"{(document.Direction != PumlDirection.TopToBottom ? "left to right direction\n" : string.Empty)}"));
            await stream.WriteAsync(Encoding.GetBytes($"{document.LineMode switch { PumlLineMode.Orthogonal => "skinparam linetype ortho\n", PumlLineMode.Polyline => "skinparam linetype polyline\n" }}"));
            if(!string.IsNullOrWhiteSpace(document.Styles)) await stream.WriteAsync(Encoding.GetBytes($"{document.Styles}\n"));

            foreach (var (clip, layers) in document.Clips)
            {
                var puml = clip.ToString(layers);

                await stream.WriteAsync(Encoding.GetBytes(puml));
            }

            if(!string.IsNullOrWhiteSpace(document.FooterNote)) await stream.WriteAsync(Encoding.GetBytes($"\nnote as footer\n\t{document.FooterNote}\nend note"));
            await stream.WriteAsync(Encoding.GetBytes("\n@enduml"));

            await stream.FlushAsync();

            return stream;
        }

    }
}
