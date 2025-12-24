using KaynakMakinesi.Core.Abstractions;
using KaynakMakinesi.Core.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace KaynakMakinesi.Infrastructure.Settings
{
    public sealed class JsonFileSettingsStore : ISettingsStore<AppSettings>
    {
        private readonly string _path;
        private readonly object _lock = new object();

        public event EventHandler SettingsChanged;

        public JsonFileSettingsStore(string path)
        {
            _path = path;
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
        }

        public AppSettings Load()
        {
            lock (_lock)
            {
                AppSettings settings;
                
                if (!File.Exists(_path))
                {
                    settings = new AppSettings();
                    Save(settings);
                    return settings;
                }

                try
                {
                    var json = File.ReadAllText(_path, Encoding.UTF8);
                    settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                    
                    // Validation - ayarlar geçersizse default'a dön
                    try
                    {
                        settings.Validate();
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Log edilebilir ama şimdilik sadece default'a dön
                        System.Diagnostics.Debug.WriteLine($"UYARI: Geçersiz ayarlar tespit edildi: {ex.Message}. Default ayarlar kullanılacak.");
                        settings = new AppSettings();
                        settings.Validate(); // Default ayarlar da geçerli olmalı
                    }
                    
                    return settings;
                }
                catch (JsonException ex)
                {
                    // JSON parse hatası - default ayarlara dön
                    System.Diagnostics.Debug.WriteLine($"UYARI: Ayar dosyası okunamadı: {ex.Message}. Default ayarlar kullanılacak.");
                    settings = new AppSettings();
                    Save(settings); // Bozuk dosyayı düzelt
                    return settings;
                }
            }
        }

        public void Save(AppSettings settings)
        {
            lock (_lock)
            {
                // Kaydedilmeden önce validation yap
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));
                
                settings.Validate();

                var json = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);

                // atomic write: temp -> replace
                var tmp = _path + ".tmp";
                File.WriteAllText(tmp, json, Encoding.UTF8);

                if (File.Exists(_path))
                    File.Replace(tmp, _path, _path + ".bak");
                else
                    File.Move(tmp, _path);

                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
