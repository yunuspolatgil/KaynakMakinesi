using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KaynakMakinesi.Core.Tags
{
    public interface ITagService
    {
        /// <summary>
        /// Tag'i ada göre oku (önce cache'den, yoksa PLC'den)
        /// </summary>
        Task<TagReadResult> ReadTagAsync(string tagName, CancellationToken ct = default);

        /// <summary>
        /// Tag'e yaz
        /// </summary>
        Task<bool> WriteTagAsync(string tagName, object value, CancellationToken ct = default);

        /// <summary>
        /// Tag'e metin olarak yaz
        /// </summary>
        Task<bool> WriteTagTextAsync(string tagName, string valueText, CancellationToken ct = default);

        /// <summary>
        /// Birden fazla tag'i toplu oku
        /// </summary>
        Task<Dictionary<string, TagReadResult>> ReadTagsAsync(IEnumerable<string> tagNames, CancellationToken ct = default);

        ///// <summary>
        ///// Tüm kayýtlý tag'larý listele
        ///// </summary>
        //List<TagDefinition> GetAllTags();

        ///// <summary>
        ///// Gruba göre tag'larý listele
        ///// </summary>
        //List<TagDefinition> GetTagsByGroup(string groupName);

        /// <summary>
        /// Tag'in son deðerini cache'den al (PLC'ye gitmez)
        /// </summary>
        TagReadResult GetCachedValue(string tagName);

        /// <summary>
        /// Tag'i cache'e zorla ekle/güncelle
        /// </summary>
        void SetCachedValue(string tagName, object value, DateTime timestamp);

        /// <summary>
        /// Tag cache'ini temizle
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Tag güncellendiðinde tetiklenen event
        /// </summary>
        event EventHandler<TagUpdatedEventArgs> TagUpdated;
    }

    public class TagReadResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public object Value { get; set; }
        public DateTime Timestamp { get; set; }
        public string TagName { get; set; }
        public string ValueAsString => Value?.ToString() ?? "";
    }

    public class TagUpdatedEventArgs : EventArgs
    {
        public string TagName { get; set; }
        public TagReadResult Result { get; set; }
    }
}