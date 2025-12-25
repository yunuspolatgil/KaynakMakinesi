namespace KaynakMakinesi.Core.Motor
{
    /// <summary>
    /// Motor kalibrasyon sürecinin durumlarý
    /// </summary>
    public enum KalibrasyonDurum
    {
        /// <summary>
        /// Baþlangýç durumu - Kullanýcý iþlem bekliyor
        /// </summary>
        Bekliyor = 0,
        
        /// <summary>
        /// Motor home pozisyonuna gidiyor
        /// </summary>
        HomeGidiyor = 1,
        
        /// <summary>
        /// Home tamamlandý, manuel hareket aþamasýnda
        /// </summary>
        ManuelHareket = 2,
        
        /// <summary>
        /// Kullanýcý ölçülen pozisyon deðerini giriyor
        /// </summary>
        OlculenPozisyonGirisi = 3,
        
        /// <summary>
        /// Kalibrasyon komutu PLC'ye gönderiliyor
        /// </summary>
        KalibrasyonYapiliyor = 4,
        
        /// <summary>
        /// Rampa ayarlarý PLC'ye gönderiliyor
        /// </summary>
        RampaAyarlaniyor = 5,
        
        /// <summary>
        /// Pozisyon test hareketi yapýlýyor
        /// </summary>
        PozisyonTest = 6,
        
        /// <summary>
        /// Kalibrasyon baþarýyla tamamlandý
        /// </summary>
        Tamamlandi = 7,
        
        /// <summary>
        /// Hata durumu - Ýþlem iptal edildi
        /// </summary>
        Hata = 99
    }
}
