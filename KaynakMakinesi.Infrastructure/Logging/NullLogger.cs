using System;
using KaynakMakinesi.Core.Logging;

namespace KaynakMakinesi.Infrastructure.Logging
{
    /// <summary>
    /// Null Object Pattern - hiçbir þey yapmayan logger
    /// Logger olmadýðýnda null reference hatasý almamak için kullanýlýr
    /// </summary>
    public sealed class NullLogger : IAppLogger
    {
        public static readonly IAppLogger Instance = new NullLogger();

        private NullLogger() { }

        public void Log(LogLevel level, string source, string message, Exception ex = null)
        {
            // Hiçbir þey yapma
        }

        public void Trace(string source, string message)
        {
            // Hiçbir þey yapma
        }

        public void Debug(string source, string message)
        {
            // Hiçbir þey yapma
        }

        public void Info(string source, string message)
        {
            // Hiçbir þey yapma
        }

        public void Warn(string source, string message)
        {
            // Hiçbir þey yapma
        }

        public void Error(string source, string message, Exception ex = null)
        {
            // Hiçbir þey yapma
        }

        public void Fatal(string source, string message, Exception ex = null)
        {
            // Hiçbir þey yapma
        }
    }
}
