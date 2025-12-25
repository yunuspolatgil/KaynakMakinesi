using KaynakMakinesi.Core.Tags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KaynakMakinesi.Core.Motor
{
    /// <summary>
    /// Motor konfigürasyon sýnýfý - Her motor için ayarlar
    /// 
    /// NOT: Tag tanýmlarý artýk HARDCODE EDÝLMÝYOR!
    /// Tüm tag'ler SQLite veritabanýndan yüklenir (Tag Manager ile yönetilir)
    /// </summary>
    public class MotorConfig
    {
        /// <summary>
        /// Motor kýsa adý (K0, K1, K2, vb.)
        /// </summary>
        public string MotorName { get; set; }
        
        /// <summary>
        /// Motor görünen adý (Torç Sað, Torç Sol, vb.)
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Tag prefix (K0_, K1_, vb.)
        /// </summary>
        public string Prefix => MotorName + "_";
        
        // ===== POZÝSYON LÝMÝTLERÝ =====
        public float MinPozisyon { get; set; } = -1000f;
        public float MaxPozisyon { get; set; } = 1000f;
        
        // ===== HIZ LÝMÝTLERÝ =====
        public int MinHiz { get; set; } = 100;
        public int MaxHiz { get; set; } = 5000;
        
        // ===== RAMPA LÝMÝTLERÝ =====
        public float MinRampa { get; set; } = 50f;
        public float MaxRampa { get; set; } = 2000f;
        
        // ===== TIMEOUT AYARLARI (milisaniye) =====
        public int HomeTimeout { get; set; } = 30000;        // 30 saniye
        public int KalibrasyonTimeout { get; set; } = 10000;  // 10 saniye
        public int HareketTimeout { get; set; } = 60000;      // 60 saniye
        
        /// <summary>
        /// Tag adýndan tam tag adý oluþturur
        /// Örnek: "Home_Switch" -> "K0_Home_Switch"
        /// </summary>
        public string GetTagName(string suffix)
        {
            return Prefix + suffix;
        }
        
        /// <summary>
        /// Pozisyon deðerinin limitler içinde olup olmadýðýný kontrol eder
        /// </summary>
        public bool ValidatePozisyon(float pozisyon, out string error)
        {
            error = null;
            
            if (pozisyon < MinPozisyon)
            {
                error = $"Pozisyon minimum limitin ({MinPozisyon} mm) altýnda!";
                return false;
            }
            
            if (pozisyon > MaxPozisyon)
            {
                error = $"Pozisyon maximum limitin ({MaxPozisyon} mm) üstünde!";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Hýz deðerinin limitler içinde olup olmadýðýný kontrol eder
        /// </summary>
        public bool ValidateHiz(int hiz, out string error)
        {
            error = null;
            
            if (hiz < MinHiz)
            {
                error = $"Hýz minimum limitin ({MinHiz} mm/min) altýnda!";
                return false;
            }
            
            if (hiz > MaxHiz)
            {
                error = $"Hýz maximum limitin ({MaxHiz} mm/min) üstünde!";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Rampa deðerinin limitler içinde olup olmadýðýný kontrol eder
        /// </summary>
        public bool ValidateRampa(float rampa, out string error)
        {
            error = null;
            
            if (rampa < MinRampa)
            {
                error = $"Rampa minimum limitin ({MinRampa}) altýnda!";
                return false;
            }
            
            if (rampa > MaxRampa)
            {
                error = $"Rampa maximum limitin ({MaxRampa}) üstünde!";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Önceden tanýmlý motor konfigürasyonlarý
        /// </summary>
        public static class Presets
        {
            public static MotorConfig K0_TorcSag => new MotorConfig
            {
                MotorName = "K0",
                DisplayName = "Torç Sað",
                MinPozisyon = -500f,
                MaxPozisyon = 500f,
                MinHiz = 100,
                MaxHiz = 3000,
                MinRampa = 50f,
                MaxRampa = 1500f
            };
            
            public static MotorConfig K1_TorcSol => new MotorConfig
            {
                MotorName = "K1",
                DisplayName = "Torç Sol",
                MinPozisyon = -500f,
                MaxPozisyon = 500f,
                MinHiz = 100,
                MaxHiz = 3000,
                MinRampa = 50f,
                MaxRampa = 1500f
            };
            
            public static MotorConfig K2_Dikey => new MotorConfig
            {
                MotorName = "K2",
                DisplayName = "Dikey Eksen",
                MinPozisyon = -500f,
                MaxPozisyon = 500f,
                MinHiz = 100,
                MaxHiz = 3000,
                MinRampa = 50f,
                MaxRampa = 1500f
            };
        }
    }
}
