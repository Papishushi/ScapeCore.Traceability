﻿using ScapeCore.Traceability.Syntax;
using System;

namespace ScapeCore.Traceability.Logging
{
    public interface ILogger : IAsyncDisposable
    {
        public string Name { get; set; }
        public Func<string> Template { get; }
        public ISink[] Sinks { get; }

        public void Log(string sinkName, string? format, bool isOverwritingLog = false, params object[] substitutions);
    }
}