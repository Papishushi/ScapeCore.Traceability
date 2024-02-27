using System.Text;
using ScapeCore.Traceability.Syntax;

namespace ScapeCore.Traceability.Logging
{
    public sealed record TerminalLogger : ILogger
    {
        private string _name = string.Empty;
        private readonly List<ISink> _sinks = [];
        private static readonly object _lock = new();
        public required string Name { get => _name; set => _name = value; }
        public string Template { get => $"[{DateTime.Now.ToShortTimeString()}]"; }
        public ISink[] Sinks { get => [.. _sinks]; }
        private readonly DirectoryInfo _directory;
        public required DirectoryInfo Directory { get => _directory; init => _directory = value; }
        public CommandParser LinkedParser { get; init; }
        private readonly List<(ISink sink, (StreamWriter output, StreamWriter self) writers)?> _outputStreams = [];

        public TerminalLogger(params ISink[] sinks)
        {
            _sinks.AddRange(sinks);
            foreach(var sink in sinks)
                _outputStreams.Add((sink,
                                    (new StreamWriter(sink.OutputStream ?? throw new ArgumentNullException(message: "Sink output stream is null.", null), 
                                    leaveOpen: true, encoding: Encoding.Default)
                                    { 
                                        AutoFlush = true, 
                                        NewLine = "\r\n" 
                                    }, new StreamWriter(sink.SelfStream ?? throw new ArgumentNullException(message: "Sink self stream is null.", null),
                                    leaveOpen: true, encoding: Encoding.Default)
                                    {
                                        AutoFlush = true,
                                        NewLine = "\r\n"
                                    })));
            Console.WriteLine("SDK CLI Encoding: " + Encoding.Default.EncodingName);
            LinkedParser = new CommandParser(this);
        }

        public void Log(string sinkName, string? format, bool isOverwritingLog = false, params object[] substitutions)
        {
            var (sink, writers)= _outputStreams.Find(x => x!.Value.sink.Name.Equals(sinkName, StringComparison.OrdinalIgnoreCase)) ??
                            throw new ArgumentException($"{sinkName} was not found as a sink for this logger.", nameof(sinkName));

            Console.SetOut(writers.output);
            if (!string.IsNullOrEmpty(format))
            {
                if (isOverwritingLog)
                {
                    lock (_lock)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.Write(new string(' ', Console.BufferWidth));
                        Console.SetCursorPosition(0, Console.CursorTop);
                    }
                }
                Console.ForegroundColor = sink.Color;
                Console.Write($"[{sink.Name}]");
                Console.ResetColor();
                Console.WriteLine($"{Template} {format}", substitutions);

                Task.Run(() =>
                {
                    writers.self.Write($"[{sink.Name}]");
                    writers.self.WriteLine($"{Template} {format}", substitutions);
                });
            }

        }

        async ValueTask IAsyncDisposable.DisposeAsync() =>
            await Parallel.ForEachAsync(_outputStreams,
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