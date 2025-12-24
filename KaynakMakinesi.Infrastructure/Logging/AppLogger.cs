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
            // Null sinkleri filtrele
            _sinks = new List<ILogSink>();
            
            if (sinks != null)
            {
                foreach (var sink in sinks)
                {
                    if (sink != null)
                        _sinks.Add(sink);
                }
            }
        }

        public void Log(LogLevel level, string source, string message, Exception ex = null)
        {
            // Null safety
            if (string.IsNullOrWhiteSpace(source))
                source = "Unknown";
            
            if (string.IsNullOrWhiteSpace(message))
                message = "";

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message,
                Exception = ex?.ToString()
            };

            // Her sink için try-catch (bir sink hata verse bile diğerleri çalışsın)
            foreach (var sink in _sinks)
            {
                try 
                { 
                    sink.Emit(entry); 
                }
                catch (Exception sinkEx)
                { 
                    // Log sink hata verirse app'i öldürmeyecek
                    // Debug modunda görelim
                    System.Diagnostics.Debug.WriteLine($"LogSink hatası: {sinkEx.Message}");
                }
            }
        }

        public void Trace(string source, string message) => Log(LogLevel.Trace, source, message);
        public void Debug(string source, string message) => Log(LogLevel.Debug, source, message);
        public void Info(string source, string message) => Log(LogLevel.Info, source, message);
        public void Warn(string source, string message) => Log(LogLevel.Warn, source, message);
        public void Error(string source, string message, Exception ex = null) => Log(LogLevel.Error, source, message, ex);
        public void Fatal(string source, string message, Exception ex = null) => Log(LogLevel.Fatal, source, message, ex);
    }
}