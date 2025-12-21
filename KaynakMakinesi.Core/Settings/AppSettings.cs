namespace KaynakMakinesi.Core.Settings
{
    public sealed class AppSettings
    {
        public PlcSettings Plc { get; set; } = new PlcSettings();
        public DatabaseSettings Database { get; set; } = new DatabaseSettings();
        public LoggingSettings Logging { get; set; } = new LoggingSettings();
    }

    public sealed class PlcSettings
    {
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 502;
        public byte UnitId { get; set; } = 1;
        public int TimeoutMs { get; set; } = 1500;

        // Bağlantı sağlığını anlamak için periyodik okuma
        public ushort HeartbeatAddress { get; set; } = 0;
        public int HeartbeatIntervalMs { get; set; } = 750;
    }

    public sealed class DatabaseSettings
    {
        public string FileName { get; set; } = "app.db";
    }

    public sealed class LoggingSettings
    {
        public string MinLevel { get; set; } = "Info";
        public int KeepInMemory { get; set; } = 2000;
    }
}