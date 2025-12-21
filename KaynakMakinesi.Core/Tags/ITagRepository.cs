using System.Collections.Generic;

namespace KaynakMakinesi.Core.Tags
{
    public interface ITagRepository
    {
        void EnsureSchema();
        List<TagDef> ListAll();
        void UpsertMany(IEnumerable<TagDef> tags);
        void DeleteByName(string name);

    }

    public sealed class TagDef
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int PollMs { get; set; }
        public bool ReadOnly { get; set; }
        public int Address1Based { get; set; }
    }
}
