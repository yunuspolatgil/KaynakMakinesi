using System.Collections.Generic;

namespace KaynakMakinesi.Core.Tags
{
    public interface ITagRepository
    {
        void EnsureSchema();
        bool TryGetByName(string name, out TagDefinition tag);

        // Tag yönetim formu için
        List<TagDefinition> GetAll();
        void Upsert(TagDefinition tag);
        void Delete(long id);
    }
}
