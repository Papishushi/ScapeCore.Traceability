using System.Drawing;
using System.Text;
using ScapeCore.Traceability.Syntax;
using static ScapeCore.Traceability.Logging.LoggingColor;

namespace ScapeCore.Traceability.Logging
{
    public sealed record TerminalLogger : ILogger
    {
        private readonly List<ISink> _sinks = [];
        private static readonly object _lock = new();
        public required string Name { get; set; }
        public required Func<string> Template { get; set; }
        public required DirectoryInfo Directory { get; init; }
        public ISink[] Sinks { get => [.. _sinks]; }
        public CommandParser LinkedParser { get; init; }
        private readonly List<(ISink sink, (StreamWriter output, StreamWriter self) writers)?> _perSinkStreamWriters = [];


        public TerminalLogger(params ISink[] sinks)
        {
            _sinks.AddRange(sinks);
            foreach(var sink in sinks)
                _perSinkStreamWriters.Add((sink,
                                   (new StreamWriter(sink.OutputStream ?? throw new ArgumentNullException(message: "Sink output stream is null.", null), 
                                    leaveOpen: true, encoding: Encoding.Default)
                                    { 
                                        AutoFlush = true, 
                                        NewLine = "\r\n" 
                                    }, 
                                    new StreamWriter(sink.SelfStream ?? throw new ArgumentNullException(message: "Sink self stream is null.", null),
                                    leaveOpen: true, encoding: Encoding.Default)
                                    {
                                        AutoFlush = true,
                                        NewLine = "\r\n"
                                    })));
            Console.WriteLine("SDK CLI Encoding: " + Encoding.Default.EncodingName);
            LinkedParser = new CommandParser(this);
        }

        public async Task WaitForCommands() => await LinkedParser.CommandParsingLoop();

        public void Log(string sinkName, string? format, bool isOverwritingLog = false, params object[] substitutions)
        {
            var (sink, writers)= _perSinkStreamWriters.Find(x => x!.Value.sink.Name.Equals(sinkName, StringComparison.OrdinalIgnoreCase)) ??
                            throw new ArgumentException($"{sinkName} was not found as a sink for this logger.", nameof(sinkName));


            Console.SetOut(writers.output);
            if (!string.IsNullOrEmpty(format))
            {
                if (isOverwritingLog)
                    RemovePreviousLine();

                ConsoleLogContent(sink, format, substitutions);

                Task.Run(() =>
                {
                    writers.self.WriteLine($"[{sink.Name}]{Template()} {format}", substitutions);
                });
            }
            else
                RemovePreviousLine();

        }

        private static void RemovePreviousLine()
        {
            lock (_lock)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.BufferWidth));
                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }

        private void ConsoleLogContent(ISink sink, string format, object[] substitutions)
        {
            lock (_lock)
            {
                Console.WriteLine($"{Bold}{sink.Color}[{sink.Name}]{Normal}{Template()}{Normal} {format}", substitutions);
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync() =>
            await Parallel.ForEachAsync(_perSinkStreamWriters,
                async (element, cT) =>
                {
                    cT.ThrowIfCancellationRequested();

                    _sinks.Clear();

                    await element!.Value.writers.output.DisposeAsync();
                    await element!.Value.writers.self.DisposeAsync();
                    await element!.Value.sink.DisposeAsync();
                });
    }
}