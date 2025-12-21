namespace KaynakMakinesi.Infrastructure.Tags
{
    public class TagEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int PollMs { get; set; }
        public bool ReadOnly { get; set; }
    }
}