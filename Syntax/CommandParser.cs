using ScapeCore.Traceability.Logging;

namespace ScapeCore.Traceability.Syntax
{
    public record CommandParser(TerminalLogger LinkedLogger)
    {
        bool _isRunning = true;
        public readonly List<Command> AvailableCommands = [];

        ~CommandParser() => _isRunning = false;
        public async Task CommandParsingLoop()
        {
            await Task.Run(() =>
            {
                while (_isRunning)
                {
                    var cd = LinkedLogger.Directory.FullName;
                    Console.Write($" > (\\scapecore{Directory.GetCurrentDirectory().Remove(0, cd.Length)}) ");
                    var input = Console.ReadLine();
                    LinkedLogger.Log("input", input, true);

                    if (string.IsNullOrEmpty(input)) continue;

                    ParseWordStringToCommandAndExecute(new WordString(input));
                }
            });
        }
        record struct CommandParametersPair(Command? Command, List<object>? Parameters);
        private void ParseWordStringToCommandAndExecute(WordString input)
        {
            int wordCount = 0;
            int currentCommandNumberExecutionParameters = 0;

            List<CommandParametersPair> commands = [];

            Command? currentCommand = null;
            List<object> currentCommandParameters = [];

            foreach (var word in input)
            {
                wordCount++;

                if (word == "|")
                {
                    ExecuteAndClear(commands);
                    currentCommand = null;
                    continue;
                }

                if (currentCommand == null)
                {
                    currentCommand = AvailableCommands.Find(x => x.Info.Name.Equals(word, StringComparison.OrdinalIgnoreCase));
                    continue;
                }

                if (currentCommand != null && currentCommand!.Info.Name != null)
                {
                    var subCommand = currentCommand.Info.Subcommands?.ToList().Find(x => x.Info.Name.Equals(word, StringComparison.OrdinalIgnoreCase));

                    if (subCommand != null && subCommand!.Info.Name != null)
                    {
                        currentCommand = subCommand;
                        continue;
                    }
                    else
                        ProcessCommand(ref currentCommand, ref currentCommandNumberExecutionParameters, word, currentCommandParameters, commands);
                }
                else
                {
                    LinkedLogger.Log("error", $"Command {word} does not exist. Did you mispelled something?");
                    continue;
                }
            }

            ExecuteAndClear(commands);
        }


        private void ExecuteAndClear(List<CommandParametersPair> commands)
        {
            while (commands.Count > 0)
            {
                var item = commands.Last();
                item.Command?.DefaultExecution([.. item.Parameters]);
                commands.Remove(item);
            }
            commands.Clear();
        }

        private void ProcessCommand(ref Command? currentCommand, ref int currentCommandNumberExecutionParameters, string word, List<object> currentCommandParameters, List<CommandParametersPair> commands)
        {
            if (currentCommand!.Info.NumberOfExecutionParameters > 0)
            {
                currentCommandParameters.Add(word);
                if (++currentCommandNumberExecutionParameters == currentCommand.Info.NumberOfExecutionParameters)
                {
                    commands.Add(new(currentCommand, currentCommandParameters));
                    currentCommandNumberExecutionParameters = 0;
                    currentCommand = currentCommand.Info.Parent;
                }
            }
            else
            {
                commands.Add(new(currentCommand, null));
                currentCommand = currentCommand.Info.Parent;
            }
        }
    }
}