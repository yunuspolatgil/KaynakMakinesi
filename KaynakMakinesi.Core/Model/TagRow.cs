using System;

namespace KaynakMakinesi.Core.Model
{
    public class TagRow
    {
        public long Id { get; set; }          // DB varsa kullanacağız
        public string Name { get; set; }      // Tag Adı
        public string Address { get; set; }   // 42013 / 00001
        public string Type { get; set; }      // Bool/UShort/Int32/Float (şimdilik string)
        public string Group { get; set; }     // Grup
        public string Description { get; set; } // Açıklama
        public int PollMs { get; set; }       // Poll (ms)
        public bool ReadOnly { get; set; }    // RO

        // Monitor için (DB’ye yazmayacağız)
        public string LastValue { get; set; }
        public string Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}