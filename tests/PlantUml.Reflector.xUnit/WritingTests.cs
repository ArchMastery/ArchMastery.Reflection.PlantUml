using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private const string OutputWriteFileSyncPuml = "./output/writeFileSync.puml";
        private const string OutputWriteFileAsyncPuml = "./output/writeFileAsync.puml";
        private const string OutputWriteDocumentSyncPuml = "./output/writeDocumentSync.puml";
        private const string OutputWriteDocumentAsyncPuml = "./output/writeDocumentAsync.puml";

        public WritingTests(ITestOutputHelper output) : base(output, LogLevel.Debug)
        {
            output.WriteLine(Path.Combine(Environment.CurrentDirectory, OutputWriteFileSyncPuml));
            output.WriteLine(Path.Combine(Environment.CurrentDirectory, OutputWriteFileAsyncPuml));
            output.WriteLine(Path.Combine(Environment.CurrentDirectory, OutputWriteDocumentSyncPuml));
            output.WriteLine(Path.Combine(Environment.CurrentDirectory, OutputWriteDocumentAsyncPuml));

            var dir = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, OutputWriteFileSyncPuml));

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
                clipPuml += clip.ToString(Layers.TypeEnd);
            }
            foreach (var type in types)
            {
                TypeHolder typeHolder = new(type);

                Assert.NotNull(typeHolder);
                var clip = typeHolder.Generate(Layers.Relationships | Layers.Inheritance, true);
                Assert.NotNull(clip);
                clips.Add((clip, Layers.Relationships | Layers.Inheritance));
                clipPuml += clip.ToString(Layers.Relationships | Layers.Inheritance);
            }

            clipPuml += "\n@enduml";

            return (clipPuml, clips);
        }

        [Theory]
        [InlineData(typeof(TestClass<>), typeof(Extensions), typeof(TestBase<>), typeof(MyEntity))]
        public void WriteDocumentSync(params Type[] types)
        {
            if(File.Exists(OutputWriteDocumentSyncPuml)) File.Delete(OutputWriteDocumentSyncPuml);

            var writer = new PumlWriter();

            var (clipPuml, clips) = GeneratePuml(types);

            var document = new PumlDocument
                           {
                               Clips = clips,
                               Title = Path.GetFileName(OutputWriteDocumentSyncPuml),
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

            writer.WriteFile(OutputWriteDocumentSyncPuml, document).Close();

            var puml = File.ReadAllText(OutputWriteDocumentSyncPuml);

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
            if(File.Exists(OutputWriteDocumentAsyncPuml)) File.Delete(OutputWriteDocumentAsyncPuml);

            var writer = new PumlWriter();

            var (clipPuml, clips) = GeneratePuml(types);

            var document = new PumlDocument
                           {
                               Clips = clips,
                               Title = Path.GetFileName(OutputWriteDocumentAsyncPuml),
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

            (await writer.WriteFileAsync(OutputWriteDocumentAsyncPuml, document)).Close();

            var puml = await File.ReadAllTextAsync(OutputWriteDocumentAsyncPuml);

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
                               Title = Path.GetFileName(OutputWriteDocumentSyncPuml),
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
                               Title = Path.GetFileName(OutputWriteDocumentAsyncPuml),
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
            if(File.Exists(OutputWriteFileSyncPuml)) File.Delete(OutputWriteFileSyncPuml);

            var writer = new PumlWriter();

            var (clipPuml, clips) = GeneratePuml(types);

            writer.WriteFile(OutputWriteFileSyncPuml, clips).Close();

            var puml = File.ReadAllText(OutputWriteFileSyncPuml);

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
            if(File.Exists(OutputWriteFileAsyncPuml)) File.Delete(OutputWriteFileAsyncPuml);

            var writer = new PumlWriter();

            var (clipPuml, clips) = GeneratePuml(types);

            (await writer.WriteFileAsync(OutputWriteFileAsyncPuml, clips)).Close();

            var puml = await File.ReadAllTextAsync(OutputWriteFileAsyncPuml);

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
