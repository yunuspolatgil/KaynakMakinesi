using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KaynakMakinesi.Core.Entities
{
    /// <summary>
    /// Dinamik Tag Entity - Kullanýcýnýn tanýmladýðý tag'leri temsil eder
    /// Sabit alanlar: Name, Address, DataType, GroupName
    /// Dinamik alanlar: MetadataJson içinde key-value olarak saklanýr
    /// </summary>
    public class TagEntity : EntityBase
    {
        #region Sabit Alanlar (Her tag'de MUTLAKA olmalý)
        
        /// <summary>
        /// Tag adý - Benzersiz identifier (örn: "Motor_K0_Speed", "CPU_IP1")
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Modbus adresi veya operand (örn: "40001", "MW100", "IP1", "42019")
        /// </summary>
        public string Address { get; set; }
        
        /// <summary>
        /// Veri tipi: "Bool", "UShort", "Int32", "Float"
        /// </summary>
        public string DataType { get; set; } = "UShort";
        
        /// <summary>
        /// Tag grubu (örn: "Motor_K0", "Sensörler", "Ayarlar")
        /// </summary>
        public string GroupName { get; set; }
        
        #endregion
        
        #region Dinamik Alanlar (JSON)
        
        /// <summary>
        /// Kullanýcýnýn eklediði ek özellikler (JSON formatýnda)
        /// Örnekler: {"Description": "Motor hýzý", "PollMs": 250, "ReadOnly": false, "Unit": "RPM"}
        /// </summary>
        public string MetadataJson { get; set; }
        
        #endregion
        
        #region Helper Metodlar
        
        /// <summary>
        /// Metadata'yý Dictionary olarak döndürür
        /// </summary>
        public Dictionary<string, object> GetMetadata()
        {
            if (string.IsNullOrWhiteSpace(MetadataJson))
                return new Dictionary<string, object>();
            
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson) 
                       ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }
        
        /// <summary>
        /// Metadata'yý Dictionary'den set eder
        /// </summary>
        public void SetMetadata(Dictionary<string, object> metadata)
        {
            if (metadata == null)
            {
                MetadataJson = null;
                return;
            }
            
            MetadataJson = JsonConvert.SerializeObject(metadata);
        }
        
        /// <summary>
        /// Metadata'dan bir deðer okur (generic)
        /// </summary>
        public T GetMetadataValue<T>(string key, T defaultValue = default)
        {
            var meta = GetMetadata();
            if (!meta.TryGetValue(key, out var value))
                return defaultValue;
            
            if (value == null)
                return defaultValue;
            
            try
            {
                // Tip uyumluysa direkt cast
                if (value is T typedValue)
                    return typedValue;
                
                // JSON deserialize'dan gelen long -> int dönüþümü için
                if (typeof(T) == typeof(int) && value is long longValue)
                    return (T)(object)(int)longValue;
                
                // Diðer dönüþümler
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Metadata'ya bir deðer yazar
        /// </summary>
        public void SetMetadataValue(string key, object value)
        {
            var meta = GetMetadata();
            meta[key] = value;
            SetMetadata(meta);
            UpdatedAt = DateTime.Now;
        }
        
        /// <summary>
        /// Metadata'dan bir deðer siler
        /// </summary>
        public bool RemoveMetadataValue(string key)
        {
            var meta = GetMetadata();
            bool removed = meta.Remove(key);
            if (removed)
            {
                SetMetadata(meta);
                UpdatedAt = DateTime.Now;
            }
            return removed;
        }
        
        #endregion
        
        #region Convenience Properties (Metadata'dan okur)
        
        /// <summary>
        /// Tag açýklamasý (Metadata'dan)
        /// </summary>
        public string Description
        {
            get => GetMetadataValue<string>("Description", "");
            set => SetMetadataValue("Description", value);
        }
        
        /// <summary>
        /// Polling interval (ms) (Metadata'dan)
        /// </summary>
        public int PollMs
        {
            get => GetMetadataValue<int>("PollMs", 250);
            set => SetMetadataValue("PollMs", value);
        }
        
        /// <summary>
        /// ReadOnly flag (Metadata'dan)
        /// </summary>
        public bool ReadOnly
        {
            get => GetMetadataValue<bool>("ReadOnly", false);
            set => SetMetadataValue("ReadOnly", value);
        }
        
        /// <summary>
        /// Scale faktörü (Metadata'dan)
        /// </summary>
        public double Scale
        {
            get => GetMetadataValue<double>("Scale", 1.0);
            set => SetMetadataValue("Scale", value);
        }
        
        /// <summary>
        /// Offset deðeri (Metadata'dan)
        /// </summary>
        public double Offset
        {
            get => GetMetadataValue<double>("Offset", 0.0);
            set => SetMetadataValue("Offset", value);
        }
        
        /// <summary>
        /// Birim (örn: "RPM", "°C", "Bar") (Metadata'dan)
        /// </summary>
        public string Unit
        {
            get => GetMetadataValue<string>("Unit", "");
            set => SetMetadataValue("Unit", value);
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Entity validasyonu
        /// </summary>
        public bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                error = "Tag adý boþ olamaz.";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(Address))
            {
                error = $"Tag '{Name}' için adres boþ olamaz.";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(DataType))
            {
                error = $"Tag '{Name}' için veri tipi boþ olamaz.";
                return false;
            }
            
            // Geçerli veri tipleri kontrolü
            var validTypes = new[] { "Bool", "UShort", "Int32", "Float" };
            if (Array.IndexOf(validTypes, DataType) == -1)
            {
                error = $"Tag '{Name}' için geçersiz veri tipi: '{DataType}'. Geçerli tipler: {string.Join(", ", validTypes)}";
                return false;
            }
            
            error = null;
            return true;
        }
        
        #endregion
        
        public override string ToString()
        {
            return $"TagEntity(Name='{Name}', Address='{Address}', Type='{DataType}', Group='{GroupName}')";
        }
    }
}
