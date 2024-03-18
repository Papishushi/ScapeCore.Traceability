using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ScapeCore.Traceability.Syntax;

using static ScapeCore.Traceability.Logging.LoggingColor;

namespace ScapeCore.Traceability.Logging
{
    public sealed record TerminalLogger : ILogger
    {
        private readonly List<(uint index, ISink sink)> _sinks = [];
        private readonly List<(ISink sink, (StreamWriter output, StreamWriter self) writers)?> _perSinkStreamWriters = [];
        private static readonly object _lock = new();

        public required uint MinimumLoggingLevel { get; set; }
        public required string Name { get; set; }
        public required Func<string> Template { get; set; }

        public required DirectoryInfo Directory { get; init; }
        public CommandParser LinkedParser { get; init; }

        public ISink[] Sinks { get => [.. _sinks.Select(x => x.sink)]; }

        private TerminalLogger() => _ = 0;
        public TerminalLogger(params (uint index, ISink sink)[] sinks)
        {
            _sinks.AddRange(sinks);
            foreach(var sink in _sinks.Select(x => x.sink))
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



        public void AddSink(uint index, ISink sink) => _sinks.Add((index, sink));
        public void RemoveSink(string sinkName) => _sinks.Remove(_sinks.Find(x => x.sink.Name == sinkName));

        public async Task WaitForCommands() => await LinkedParser.CommandParsingLoop();

        public void Log(string sinkName, string? format, bool isOverwritingLog = false, params object[] substitutions)
        {
            var (sink, writers)= _perSinkStreamWriters.Find(x => x!.Value.sink.Name.Equals(sinkName, StringComparison.OrdinalIgnoreCase)) ??
                            throw new ArgumentException($"{sinkName} was not found as a sink for this logger.", nameof(sinkName));


            Console.SetOut(writers.output);
            if (!string.IsNullOrEmpty(format))
            {
                var index = GetSinkIndex(sink);
                if (index <= MinimumLoggingLevel)
                {
                    if (isOverwritingLog)
                        RemovePreviousLine();

                    ConsoleLogContent(sink, format, substitutions);
                }

                Task.Run(() =>
                {
                    writers.self.WriteLine($"[{sink.Name}]{Template()} {format}", substitutions);
                });
            }
            else
                RemovePreviousLine();

            RemoveNextLine(); 
        }

        public int GetSinkIndex(ISink sink)
        {
            try
            {
                var temp = _sinks.Find(x => x.sink.Name == sink.Name);
                if (temp == default) return -1;
                else return (int)temp.index;
            }
            catch (ArgumentNullException)
            {
                return -1;
            }
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

        private static void RemoveNextLine()
        {
            lock (_lock)
            {
                Console.SetCursorPosition(0, Console.CursorTop + 1);
                Console.Write(new string(' ', Console.BufferWidth));
                Console.SetCursorPosition(0, Console.CursorTop -1);
            }
        }

        private void ConsoleLogContent(ISink sink, string format, object[] substitutions)
        {
            lock (_lock)
            {
                Console.WriteLine($"{Bold}{sink.Color}[{sink.Name}]{Normal}{Template()}{Normal} {format}", substitutions);
            }
        }

        public async ValueTask DisposeAsync() =>
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