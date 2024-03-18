using ScapeCore.Traceability.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using static ScapeCore.Traceability.Logging.LoggingColor;

namespace ScapeCore.Traceability.Syntax
{
    public readonly record struct CommandInfo(string Name, int NumberOfExecutionParameters, string Help)
    {

    }
    public record class Command(CommandInfo Info, Action<object[]?> DefaultExecution, Command? Parent = null)
    {
        public Command? Parent { get; init; } = Parent;
        private readonly List<Command> _subcommands = [];
        public Command[] Subcommands { get => [.. _subcommands]; }

        public void AddSubcommand(Command command) => _subcommands.Add(command);
        public void AddRangeSubCommand(params Command[] commands) => _subcommands.AddRange(commands);
        public void RemoveSubcommand(Command command) => _subcommands.Remove(command);
        public void ClearSubcommands() => _subcommands.Clear();

        public bool supress = false;

        public Command(CommandInfo info, Action<object[]?> defaultExecution, Command? parent = null, bool addHelp = true) : this(info, defaultExecution, parent) 
        {
            if (addHelp)
                _subcommands.Add(new HelpCommand(this));
        }

        public static void PropagateSupress(bool supress, Command? parent = null)
        {
            var lastParent = parent;
            while (true)
            {
                if (lastParent != null)
                    lastParent.supress = supress;
                else
                    break;
                lastParent = lastParent.Parent;
            }
        }

        public string? ToString(int depth = 0)
        {
            string indentation = "\t";
            for (int i = 0; i < depth; i++)
                indentation += "\t";
            var sb = new StringBuilder();
            if (Subcommands.Length > 0)
                foreach (var subcommand in Subcommands.OrderBy(n => n.Info.Name))
                    sb.Append("\n" + indentation + subcommand.ToString(depth + 1));
                
            return $"{Yellow + Underline + Info.Name + Normal} ({Yellow + Bold + Info.NumberOfExecutionParameters + Normal}) : {Info.Help} {(sb.Length == 0 ? string.Empty : sb)}";
        }
    }
}