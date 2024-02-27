namespace ScapeCore.Traceability.Logging
{
    public sealed class ConsoleErrorSink : ISink
    {
        private readonly string _name = string.Empty;
        private readonly ConsoleColor _color = ConsoleColor.DarkRed;
        private BufferedStream? _stream = new(Console.OpenStandardError());
        private BufferedStream? _selfStream = new(new MemoryStream());
        private bool disposedValue;

        public ConsoleErrorSink(string name, ConsoleColor color)
        {
            _name = name;
            _color = color;
        }

        public string Name { get => _name; }
        public ConsoleColor Color { get => _color; }
        public Stream? OutputStream { get => _stream; }
        public Stream? SelfStream { get => _selfStream; }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _stream?.Dispose();
                    _stream = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (_stream is not null)
                await _stream.DisposeAsync().ConfigureAwait(false);

            _stream = null;
        }

    }
}