namespace ScapeCore.Traceability.Logging
{
    public interface ISink : IDisposable, IAsyncDisposable
    {
        public string Name { get; }
        public ConsoleColor Color { get; }
        public Stream? OutputStream { get; }
        public Stream? SelfStream { get; }
    }
}