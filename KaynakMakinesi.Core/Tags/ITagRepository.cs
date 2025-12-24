using System.Collections.Generic;

namespace KaynakMakinesi.Core.Tags
{
    public interface ITagRepository
    {
        void EnsureSchema();
        List<TagDefinition> ListAll();
        void UpsertMany(IEnumerable<TagDefinition> tags);
        void DeleteByName(string name);
    }
}
