using System;

namespace KaynakMakinesi.Core.Motor
{
    /// <summary>
    /// Kalibrasyon kayýt modeli - Tüm kalibrasyon iþlemleri loglanýr
    /// </summary>
    public class KalibrasyonLog
    {
        /// <summary>
        /// Benzersiz ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Motor adý (K0, K1, vb.)
        /// </summary>
        public string MotorName { get; set; }
        
        /// <summary>
        /// Kalibrasyon tarihi
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Kullanýcý adý (opsiyonel)
        /// </summary>
        public string KullaniciAdi { get; set; }
        
        /// <summary>
        /// Ölçülen pozisyon deðeri
        /// </summary>
        public float OlculenPozisyon { get; set; }
        
        /// <summary>
        /// PLC'den dönen çýkýþ pozisyonu
        /// </summary>
        public float CikisPozisyonu { get; set; }
        
        /// <summary>
        /// Rampa hýzlanma deðeri
        /// </summary>
        public float RampaHizlanma { get; set; }
        
        /// <summary>
        /// Rampa yavaþlama deðeri
        /// </summary>
        public float RampaYavaslama { get; set; }
        
        /// <summary>
        /// Test için gidilen pozisyon
        /// </summary>
        public float? TestPozisyon { get; set; }
        
        /// <summary>
        /// Test sonucu baþarýlý mý?
        /// </summary>
        public bool TestBasarili { get; set; }
        
        /// <summary>
        /// Kalibrasyon baþarýlý mý?
        /// </summary>
        public bool Basarili { get; set; }
        
        /// <summary>
        /// Hata mesajý (varsa)
        /// </summary>
        public string HataMesaji { get; set; }
        
        /// <summary>
        /// Notlar (opsiyonel)
        /// </summary>
        public string Notlar { get; set; }
    }
}
