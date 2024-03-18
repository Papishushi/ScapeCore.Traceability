using ScapeCore.Traceability.Logging;
using ScapeCore.Traceability.Syntax;
using System;

record class HelpCommand : Command
{
    public HelpCommand(Command? parent = null) : base(new("-H", 0, $"Displays {parent?.Info.Name} help."), (_) =>
    {
        var val = parent?.ToString();
        Console.WriteLine(val);
        PropagateSupress(true, parent);
    }, parent) => _ = 0;
}