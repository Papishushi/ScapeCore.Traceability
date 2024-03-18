using System;
using System.IO;

namespace ScapeCore.Traceability.Logging
{
    public interface ISink : IDisposable, IAsyncDisposable
    {
        public string Name { get; }
        public LoggingColor Color { get; }
        public Stream? OutputStream { get; }
        public Stream? SelfStream { get; }
    }
}