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
                if (!File.Exists(_path))
                {
                    var defaults = new AppSettings();
                    Save(defaults);
                    return defaults;
                }

                var json = File.ReadAllText(_path, Encoding.UTF8);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            lock (_lock)
            {
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
