using System;
using System.Collections.Generic;
using KaynakMakinesi.Core.Logging;

namespace KaynakMakinesi.Infrastructure.Logging
{
    public sealed class AppLogger : IAppLogger
    {
        private readonly List<ILogSink> _sinks;

        public AppLogger(params ILogSink[] sinks)
        {
            _sinks = new List<ILogSink>(sinks ?? Array.Empty<ILogSink>());
        }

        public void Log(LogLevel level, string source, string message, Exception ex = null)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message,
                Exception = ex?.ToString()
            };

            foreach (var s in _sinks)
            {
                try { s.Emit(entry); }
                catch { /* log sink hata verirse app’i öldürmeyecek */ }
            }
        }

        public void Info(string source, string message) => Log(LogLevel.Info, source, message);
        public void Warn(string source, string message) => Log(LogLevel.Warn, source, message);
        public void Error(string source, string message, Exception ex = null) => Log(LogLevel.Error, source, message, ex);
    }
}