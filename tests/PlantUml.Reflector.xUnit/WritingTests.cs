using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Divergic.Logging.Xunit;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

#nullable enable
namespace PlantUml.Reflector.xUnit
{
    public class WritingTests : LoggingTestsBase
    {
        private readonly string _writeFileSync;
        private readonly string _writeFileAsync;
        private readonly string _writeDocumentSync;
        private readonly string _writeDocumentAsync;

        public WritingTests(ITestOutputHelper output) : base(output, LogLevel.Debug)
        {
            const string outputWriteFileSyncPuml = "output/writeFileSync.puml";
            const string outputWriteFileAsyncPuml = "output/writeFileAsync.puml";
            const string outputWriteDocumentSyncPuml = "output/writeDocumentSync.puml";
            const string outputWriteDocumentAsyncPuml = "output/writeDocumentAsync.puml";

            _writeFileSync = Path.Combine(Environment.CurrentDirectory, outputWriteFileSyncPuml);
            _writeFileAsync = Path.Combine(Environment.CurrentDirectory, outputWriteFileAsyncPuml);
            _writeDocumentSync = Path.Combine(Environment.CurrentDirectory, outputWriteDocumentSyncPuml);
            _writeDocumentAsync = Path.Combine(Environment.CurrentDirectory, outputWriteDocumentAsyncPuml);

            if (OperatingSystem.IsWindows())
            {
                _writeFileSync = _writeFileSync.Replace("/", "\\");
                _writeFileAsync = _writeFileAsync.Replace("/", "\\");
                _writeDocumentSync = _writeDocumentSync.Replace("/", "\\");
                _writeDocumentAsync = _writeDocumentAsync.Replace("/", "\\");
            }

            var dir = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, _writeFileSync));

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        private static (string, IEnumerable<(PumlClip clip, Layers layers)> clips) GeneratePuml(params Type[] types)
        {
            var clipPuml = "@startuml\n";
            var clips = new List<(PumlClip clip, Layers layers)>();
            foreach (var type in types)
            {
                TypeHolder typeHolder = new(type);

                Assert.NotNull(typeHolder);
                var clip = typeHolder.Generate(Layers.TypeEnd, true);
                Assert.NotNull(clip);
                clips.Add((clip, Layers.TypeEnd));
            }
            foreach (var type in types)
            {
                TypeHolder typeHolder = new(type);

                Assert.NotNull(typeHolder);
                var clip = typeHolder.Generate(Layers.Relationships | Layers.Inheritance, true);
                Assert.NotNull(clip);
                clips.Add((clip, Layers.Relationships | Layers.Inheritance));
            }

            foreach (var (clip, layers) in clips)
            {
                var puml = clip.ToString(layers);

                clipPuml += puml;
            }

            clipPuml += "\n@enduml";

            return (clipPuml, clips);
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public void WriteDocumentSync(params Type[] types)
        {
            if (File.Exists(_writeDocumentSync)) File.Delete(_writeDocumentSync);

            var writer = new PumlWriter();

            var (clipPuml, clips) = GeneratePuml(types);

            var document = new PumlDocument
            {
                Clips = clips,
                Title = Path.GetFileName(_writeDocumentSync),
                Direction = PumlDirection.TopToBottom,
                LineMode = PumlLineMode.Orthogonal,
                HeaderComment = "This is a header comment.",
                FooterNote = "Testing...",
                Styles = @"skinparam class {
    BackgroundColor #cfcfcf
    ArrowColor Navy
    BorderColor Navy
}" + Environment.NewLine
            };

            writer.WriteFile(_writeDocumentSync, document).Close();

            var puml = File.ReadAllText(_writeDocumentSync);

            Output.WriteLine(new string('=', 80));
            Output.WriteLine(puml);
            Output.WriteLine(new string('=', 80));
            //Output.WriteLine(clipPuml);
            //Output.WriteLine(new string('=', 80));

            puml.Should().NotBeNullOrWhiteSpace();
            //puml.Should().BeEquivalentTo(clipPuml);
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public async Task WriteDocumentAsync(params Type[] types)
        {
            if (File.Exists(_writeDocumentAsync)) File.Delete(_writeDocumentAsync);

            var writer = new PumlWriter();

            var (clipPuml, clips) = GeneratePuml(types);

            var document = new PumlDocument
            {
                Clips = clips,
                Title = Path.GetFileName(_writeDocumentAsync),
                Direction = PumlDirection.TopToBottom,
                LineMode = PumlLineMode.Orthogonal,
                HeaderComment = "This is a header comment.",
                FooterNote = "Testing...",
                Styles = @"skinparam class {
    BackgroundColor #cfcfcf
    ArrowColor Navy
    BorderColor Navy
}" + Environment.NewLine
            };

            (await writer.WriteFileAsync(_writeDocumentAsync, document)).Close();

            var puml = await File.ReadAllTextAsync(_writeDocumentAsync);

            Output.WriteLine(new string('=', 80));
            Output.WriteLine(puml);
            Output.WriteLine(new string('=', 80));
            //Output.WriteLine(clipPuml);
            //Output.WriteLine(new string('=', 80));

            puml.Should().NotBeNullOrWhiteSpace();
            //puml.Should().BeEquivalentTo(clipPuml);
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public void WriteDocumentStreamSync(params Type[] types)
        {
            var writer = new PumlWriter();
            using var stream = new MemoryStream();

            var (clipPuml, clips) = GeneratePuml(types);

            var document = new PumlDocument
            {
                Clips = clips,
                Title = Path.GetFileName(_writeDocumentSync),
                Direction = PumlDirection.TopToBottom,
                LineMode = PumlLineMode.Orthogonal,
                HeaderComment = "This is a header comment.",
                FooterNote = "Testing...",
                Styles = @"skinparam class {
    BackgroundColor #cfcfcf
    ArrowColor Navy
    BorderColor Navy
}" + Environment.NewLine
            };

            writer.WriteStream(stream, document);

            stream.Seek(0, SeekOrigin.Begin);

            var buffer = new Span<byte>(new byte[stream.Length]);

            stream.Read(buffer);

            var puml = writer.Encoding.GetString(buffer.ToArray());

            Output.WriteLine(new string('=', 80));
            Output.WriteLine(puml);
            Output.WriteLine(new string('=', 80));
            //Output.WriteLine(clipPuml);
            //Output.WriteLine(new string('=', 80));

            puml.Should().NotBeNullOrWhiteSpace();
            //puml.Should().BeEquivalentTo(clipPuml);
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public async Task WriteDocumentStreamAsync(params Type[] types)
        {
            var writer = new PumlWriter();
            await using var stream = new MemoryStream();

            var (clipPuml, clips) = GeneratePuml(types);

            var document = new PumlDocument
            {
                Clips = clips,
                Title = Path.GetFileName(_writeDocumentAsync),
                Direction = PumlDirection.TopToBottom,
                LineMode = PumlLineMode.Orthogonal,
                HeaderComment = "This is a header comment.",
                FooterNote = "Testing...",
                Styles = @"skinparam class {
    BackgroundColor #cfcfcf
    ArrowColor Navy
    BorderColor Navy
}" + Environment.NewLine
            };

            await writer.WriteStreamAsync(stream, document);

            stream.Seek(0, SeekOrigin.Begin);

            var buffer = new Memory<byte>(new byte[stream.Length]);

            await stream.ReadAsync(buffer);

            var puml = writer.Encoding.GetString(buffer.ToArray());

            Output.WriteLine(new string('=', 80));
            Output.WriteLine(puml);
            Output.WriteLine(new string('=', 80));
            //Output.WriteLine(clipPuml);
            //Output.WriteLine(new string('=', 80));

            puml.Should().NotBeNullOrWhiteSpace();
            //puml.Should().BeEquivalentTo(clipPuml);
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public void WriteFileSync(params Type[] types)
        {
            if (File.Exists(_writeFileSync)) File.Delete(_writeFileSync);

            var writer = new PumlWriter();

            var (clipPuml, clips) = GeneratePuml(types);

            writer.WriteFile(_writeFileSync, clips).Close();

            var puml = File.ReadAllText(_writeFileSync);

            Output.WriteLine(new string('=', 80));
            Output.WriteLine(puml);
            Output.WriteLine(new string('=', 80));
            Output.WriteLine(clipPuml);
            Output.WriteLine(new string('=', 80));

            puml.Should().NotBeNullOrWhiteSpace();
            puml.Should().BeEquivalentTo(clipPuml);
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public async Task WriteFileAsync(params Type[] types)
        {
            Output.WriteLine(_writeFileAsync);
            if (File.Exists(_writeFileAsync)) File.Delete(_writeFileAsync);

            var writer = new PumlWriter();

            var (clipPuml, clips) = GeneratePuml(types);

            (await writer.WriteFileAsync(_writeFileAsync, clips)).Close();

            var puml = await File.ReadAllTextAsync(_writeFileAsync);

            Output.WriteLine(new string('=', 80));
            Output.WriteLine(puml);
            Output.WriteLine(new string('=', 80));
            Output.WriteLine(clipPuml);
            Output.WriteLine(new string('=', 80));

            puml.Should().NotBeNullOrWhiteSpace();
            puml.Should().BeEquivalentTo(clipPuml);
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public void WriteStreamSync(params Type[] types)
        {
            var writer = new PumlWriter();
            using var stream = new MemoryStream();

            var (clipPuml, clips) = GeneratePuml(types);

            writer.WriteStream(stream, clips);

            stream.Seek(0, SeekOrigin.Begin);

            var buffer = new Span<byte>(new byte[stream.Length]);

            stream.Read(buffer);

            var puml = writer.Encoding.GetString(buffer.ToArray());

            Output.WriteLine(new string('=', 80));
            Output.WriteLine(puml);
            Output.WriteLine(new string('=', 80));
            Output.WriteLine(clipPuml);
            Output.WriteLine(new string('=', 80));

            puml.Should().NotBeNullOrWhiteSpace();
            puml.Should().BeEquivalentTo(clipPuml);
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public async Task WriteStreamAsync(params Type[] types)
        {
            var writer = new PumlWriter();
            await using var stream = new MemoryStream();

            var (clipPuml, clips) = GeneratePuml(types);

            await writer.WriteStreamAsync(stream, clips);

            stream.Seek(0, SeekOrigin.Begin);

            var buffer = new Memory<byte>(new byte[stream.Length]);

            await stream.ReadAsync(buffer);

            var puml = writer.Encoding.GetString(buffer.ToArray());

            Output.WriteLine(new string('=', 80));
            Output.WriteLine(puml);
            Output.WriteLine(new string('=', 80));
            Output.WriteLine(clipPuml);
            Output.WriteLine(new string('=', 80));

            puml.Should().NotBeNullOrWhiteSpace();
            puml.Should().BeEquivalentTo(clipPuml);
        }
    }
}
