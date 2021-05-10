using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PlantUml.Reflector
{
    public class PumlWriter : TextWriter
    {
        /// <inheritdoc />
        public override Encoding Encoding { get; } = Encoding.UTF8;

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

    }
}
