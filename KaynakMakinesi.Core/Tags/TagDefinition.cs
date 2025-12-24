namespace KaynakMakinesi.Core.Tags
{
    public sealed class TagDefinition
    {
        public long Id { get; set; }
        public string Name { get; set; }            // CPU_IP0, acilstop, Home_Hiz_K0
        public int Address1Based { get; set; }      // 10001, 42013, 2
        public string Description { get; set; }
        public bool ReadOnly { get; set; }

        // opsiyonel override (çoğu boş kalır)
        public string AreaOverride { get; set; }    // "Coil" vs
        public string TypeOverride { get; set; }    // "Float" vs
        public double Scale { get; set; } = 1.0;
        public double Offset { get; set; } = 0.0;
        
        // YENİ EKLENENLER - SqliteTagRepository ile uyumluluk için
        public string Address { get; set; } = "";   // Tag'in adres string'i (MW0, MI1, vs)
        public string GroupName { get; set; } = ""; // Tag grubu
        public int PollMs { get; set; } = 250;      // Polling interval
    }
}