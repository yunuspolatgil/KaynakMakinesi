using System;

namespace KaynakMakinesi.Core.Entities
{
    /// <summary>
    /// Tüm entity'lerin base class'ý
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>
        /// Primary Key - Her entity'nin benzersiz ID'si
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// Oluþturma zamaný
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Son güncellenme zamaný
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Soft delete için - Aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Oluþturan kullanýcý (opsiyonel)
        /// </summary>
        public string CreatedBy { get; set; }
        
        /// <summary>
        /// Güncelleyen kullanýcý (opsiyonel)
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
