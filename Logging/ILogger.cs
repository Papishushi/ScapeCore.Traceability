using ScapeCore.Traceability.Syntax;

namespace ScapeCore.Traceability.Logging
{
    public interface ILogger : IAsyncDisposable
    {
        public string Name { get; set; }
        public string Template { get; }
        public ISink[] Sinks { get; }
        public CommandParser LinkedParser { get; init; }

        public void Log(string sinkName, string? format, bool isOverwritingLog = false, params object[] substitutions);
    }
}