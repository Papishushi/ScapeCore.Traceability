namespace ScapeCore.Traceability.Syntax
{
    public readonly record struct CommandInfo(string Name, Command? Parent, Command[]? Subcommands, int NumberOfExecutionParameters, string Help);
    public record class Command(CommandInfo Info, Action<object[]?> DefaultExecution)
    {
        public override string? ToString() => Info.ToString();
    }
}