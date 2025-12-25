using System;

namespace KaynakMakinesi.Core.Model
{
    /// <summary>
    /// FrmTagManager grid row model - TagEntity'yi UI'da temsil eder
    /// </summary>
    public class TagEntityRow
    {
        // Entity alanlarý
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string DataType { get; set; }
        public string GroupName { get; set; }
        
        // Sýk kullanýlan metadata alanlarý (convenience)
        public string Description { get; set; }
        public int PollMs { get; set; } = 250;
        public bool ReadOnly { get; set; }
        public double Scale { get; set; } = 1.0;
        public double Offset { get; set; }
        public string Unit { get; set; }
        
        // Monitor için (UI-only, DB'ye yazýlmaz)
        public string LastValue { get; set; }
        public string Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Dinamik alanlar için metadata JSON (görünür olmasýn)
        public string MetadataJson { get; set; }
    }
}
