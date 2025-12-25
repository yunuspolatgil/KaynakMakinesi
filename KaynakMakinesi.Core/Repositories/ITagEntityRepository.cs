using System.Collections.Generic;
using KaynakMakinesi.Core.Entities;

namespace KaynakMakinesi.Core.Repositories
{
    /// <summary>
    /// Tag Repository Interface - Tag entity'leri için özel metodlar
    /// Generic repository'den türer + ek metodlar
    /// </summary>
    public interface ITagEntityRepository : IRepository<TagEntity>
    {
        /// <summary>
        /// Tag adýna göre getirir (case-insensitive)
        /// </summary>
        TagEntity GetByName(string name);
        
        /// <summary>
        /// Grup adýna göre tag'leri getirir
        /// </summary>
        IEnumerable<TagEntity> GetByGroup(string groupName);
        
        /// <summary>
        /// Veri tipine göre tag'leri getirir
        /// </summary>
        IEnumerable<TagEntity> GetByDataType(string dataType);
        
        /// <summary>
        /// Tag adý var mý kontrol eder (case-insensitive)
        /// </summary>
        bool ExistsByName(string name);
        
        /// <summary>
        /// Tag'i adýna göre siler (soft delete)
        /// </summary>
        void RemoveByName(string name);
        
        /// <summary>
        /// Birden fazla tag'i adlarýna göre siler (soft delete)
        /// </summary>
        void RemoveByNames(IEnumerable<string> names);
        
        /// <summary>
        /// Birden fazla tag'i upsert eder (varsa güncelle, yoksa ekle)
        /// </summary>
        void UpsertMany(IEnumerable<TagEntity> tags);
        
        /// <summary>
        /// Tüm gruplarý getirir (distinct)
        /// </summary>
        IEnumerable<string> GetAllGroups();
        
        /// <summary>
        /// Aktif olmayan (soft-deleted) tag'leri getirir
        /// </summary>
        IEnumerable<TagEntity> GetInactive();
        
        /// <summary>
        /// Tag'i geri yükler (soft delete'ten)
        /// </summary>
        void Restore(long id);
    }
}
