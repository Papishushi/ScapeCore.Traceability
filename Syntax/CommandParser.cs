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
        private void ParseWordStringToCommandAndExecute(WordString input)
        {
            int i = 0, h = 0;
            Command? fCommand = null;
            Command? fSubCommand = null;

            List<object> parameterParameters = [], commandParameters = [];
            foreach (var word in input)
            {
                if (word == "|")
                {
                    if (fSubCommand == null)
                        h = ExecuteCommand(ref fCommand, commandParameters);
                    else
                        i = ExecuteCommand(ref fSubCommand, parameterParameters);
                    continue;
                }

                fCommand ??= AvailableCommands.Find(x => x.Info.Name.Equals(word, StringComparison.OrdinalIgnoreCase));

                if (fCommand != null && fCommand!.Info.Name != null)
                {
                    fSubCommand ??= fCommand.Info.Subcommands?.ToList().Find(x => x.Info.Name.Equals(word, StringComparison.OrdinalIgnoreCase));
                    if (fSubCommand != null && fSubCommand!.Info.Name != null)
                    {
                        if (fSubCommand.Info.NumberOfExecutionParameters > 0)
                        {
                            parameterParameters.Add(word);
                            if (i++ == fSubCommand.Info.NumberOfExecutionParameters || input.Last() == word)
                                i = ExecuteCommand(ref fSubCommand, parameterParameters);
                        }
                        else
                        {
                            LinkedLogger.Log("debug", fSubCommand.ToString());
                            fSubCommand.DefaultExecution(null);
                            fSubCommand = null;
                        }
                    }
                    else
                    {
                        if (fCommand.Info.NumberOfExecutionParameters > 0)
                        {
                            commandParameters.Add(word);
                            if (h++ == fCommand.Info.NumberOfExecutionParameters || input.Last() == word)
                                h = ExecuteCommand(ref fCommand, commandParameters);
                        }
                        else
                        {
                            fCommand.DefaultExecution(null);
                            fCommand = null;
                        }
                    }
                }
                else
                {
                    LinkedLogger.Log("error", $"Command {word} does not exist. Did you mispelled something?");
                    continue;
                }
            }

            static int ExecuteCommand(ref Command? fCommand, List<object> commandParameters)
            {
                fCommand?.DefaultExecution([.. commandParameters]);
                fCommand = null;
                commandParameters.Clear();
                return 0;
            }
        }
    }
}