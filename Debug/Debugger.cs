using ScapeCore.Traceability.Logging;
using ScapeCore.Traceability.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScapeCore.Traceability.Debug
{
    public static class Debugger
    {
        public const string INPUT = "INPUT";
        public const string DEBUG = "DEBUG";
        public const string ERROR = "ERROR";
        public const string WARNING = "WARNING";
        public const string INFORMATION = "INFORMATION";
        public const string VERBOSE = "VERBOSE";

        private static readonly TerminalLogger _log = new([
            (0, new ConsoleErrorSink(ERROR, new LoggingColor(255, 58, 36))),
            (1, new ConsoleErrorSink(WARNING, new LoggingColor(255, 129, 26))),
            (2, new ConsoleOutputSink(INPUT, new LoggingColor(100, 200, 208))),
            (3, new ConsoleOutputSink(INFORMATION, new LoggingColor(128, 117, 108))),
            (4, new ConsoleOutputSink(DEBUG, new LoggingColor(128, 117, 108))),
            (5, new ConsoleErrorSink(VERBOSE, new LoggingColor(50, 129, 26))),
        ])
        {
            Name = "Test",
            Directory = Directory.CreateTempSubdirectory("ScapeCore"),
            Template = () => $"{new LoggingColor(255, 253, 125)}[{DateTime.Now:T}]{LoggingColor.Default}",
            MinimumLoggingLevel = 4
        };
        public static TerminalLogger SCLog { get => _log; }

        static Debugger()
        {
            Directory.SetCurrentDirectory(_log.Directory.FullName);
            var helpCommand = new Command(new("HELP", 0, "Generates a help list that contains all the available commands."), (_) =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("Available Commands for the CLI");
                foreach (var command in _log.LinkedParser.AvailableCommands.OrderBy(n => n.Info.Name))
                    sb.AppendLine(command.ToString());
                Console.Write(sb.ToString());
            }, addHelp: true);
            var helpByCommand = new Command(new("-C", 1, "Generates the help for a specific command."), (p) =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("Available Commands for the CLI");
                var val = _log.LinkedParser.AvailableCommands.Find(x =>
                                           x.Info.Name.Equals(p?.FirstOrDefault() as string,
                                           StringComparison.OrdinalIgnoreCase))?.ToString();
                if (!string.IsNullOrEmpty(val))
                    sb.AppendLine(val);
                Console.Write(sb.ToString());
                helpCommand.suppress = true;
            }, helpCommand, true);
            helpCommand.AddSubcommand(helpByCommand);
            var clearCommand = new Command(new("CLEAR", 0, "Clears the display."), (_) => Console.Clear(), addHelp: true);

            Command[] commands = [
                helpCommand,
                clearCommand,
            ];

            _log.LinkedParser.AvailableCommands.AddRange(commands);

            _log.WaitForCommands();
        }

        public static async ValueTask DisposeAsync() => await _log.DisposeAsync();
    }
}
