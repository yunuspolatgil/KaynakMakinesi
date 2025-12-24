using System;

namespace KaynakMakinesi.Core.Settings
{
    public sealed class AppSettings
    {
        public PlcSettings Plc { get; set; } = new PlcSettings();
        public DatabaseSettings Database { get; set; } = new DatabaseSettings();
        public LoggingSettings Logging { get; set; } = new LoggingSettings();

        /// <summary>
        /// Ayarların geçerliliğini kontrol eder
        /// </summary>
        public void Validate()
        {
            if (Plc == null)
                throw new InvalidOperationException("PLC ayarları null olamaz");
            
            Plc.Validate();
            
            if (Database == null)
                throw new InvalidOperationException("Database ayarları null olamaz");
            
            Database.Validate();
            
            if (Logging == null)
                throw new InvalidOperationException("Logging ayarları null olamaz");
            
            Logging.Validate();
        }
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

        /// <summary>
        /// PLC ayarlarının geçerliliğini kontrol eder
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Ip))
                throw new InvalidOperationException("PLC IP adresi boş olamaz");

            // Basit IP format kontrolü
            var parts = Ip.Split('.');
            if (parts.Length != 4)
                throw new InvalidOperationException($"Geçersiz IP formatı: {Ip}");

            if (Port <= 0 || Port > 65535)
                throw new InvalidOperationException($"Geçersiz port numarası: {Port}. Geçerli aralık: 1-65535");

            if (UnitId < 1 || UnitId > 247)
                throw new InvalidOperationException($"Geçersiz Unit ID: {UnitId}. Geçerli aralık: 1-247");

            if (TimeoutMs < 100 || TimeoutMs > 30000)
                throw new InvalidOperationException($"Geçersiz timeout: {TimeoutMs}ms. Önerilen aralık: 100-30000ms");

            if (HeartbeatIntervalMs < 100 || HeartbeatIntervalMs > 60000)
                throw new InvalidOperationException($"Geçersiz heartbeat interval: {HeartbeatIntervalMs}ms. Önerilen aralık: 100-60000ms");
        }
    }

    public sealed class DatabaseSettings
    {
        public string FileName { get; set; } = "app.db";

        /// <summary>
        /// Database ayarlarının geçerliliğini kontrol eder
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FileName))
                throw new InvalidOperationException("Database dosya adı boş olamaz");

            // Geçersiz dosya adı karakterleri kontrolü
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            if (FileName.IndexOfAny(invalidChars) >= 0)
                throw new InvalidOperationException($"Database dosya adı geçersiz karakterler içeriyor: {FileName}");
        }
    }

    public sealed class LoggingSettings
    {
        public string MinLevel { get; set; } = "Info";
        public int KeepInMemory { get; set; } = 2000;

        /// <summary>
        /// Logging ayarlarının geçerliliğini kontrol eder
        /// </summary>
        public void Validate()
        {
            var validLevels = new[] { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" };
            if (string.IsNullOrWhiteSpace(MinLevel) || Array.IndexOf(validLevels, MinLevel) < 0)
                throw new InvalidOperationException($"Geçersiz log seviyesi: {MinLevel}. Geçerli değerler: {string.Join(", ", validLevels)}");

            if (KeepInMemory < 100 || KeepInMemory > 100000)
                throw new InvalidOperationException($"Geçersiz log bellek kapasitesi: {KeepInMemory}. Önerilen aralık: 100-100000");
        }
    }
}