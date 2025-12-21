using System;

namespace KaynakMakinesi.Core.Logging
{
    public enum LogLevel { Trace, Debug, Info, Warn, Error, Fatal }

    public sealed class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
    }

    public interface ILogSink
    {
        void Emit(LogEntry entry);
    }

    public interface IAppLogger
    {
        void Log(LogLevel level, string source, string message, Exception ex = null);
        void Info(string source, string message);
        void Warn(string source, string message);
        void Error(string source, string message, Exception ex = null);
    }
}