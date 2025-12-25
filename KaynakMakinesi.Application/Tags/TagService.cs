using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.Core.Tags;
using KaynakMakinesi.Core.Entities;
using KaynakMakinesi.Core.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KaynakMakinesi.Application.Tags
{
    public class TagService : ITagService
    {
        private readonly ITagEntityRepository _tagRepository;
        private readonly IModbusService _modbusService;
        private readonly IAppLogger _logger;
        
        // Cache için thread-safe koleksiyonlar
        private readonly ConcurrentDictionary<string, TagReadResult> _cache = new ConcurrentDictionary<string, TagReadResult>();
        private readonly ConcurrentDictionary<string, TagEntity> _tagDefinitions = new ConcurrentDictionary<string, TagEntity>();
        
        public event EventHandler<TagUpdatedEventArgs> TagUpdated;

        public TagService(ITagEntityRepository tagRepository, IModbusService modbusService, IAppLogger logger)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _modbusService = modbusService ?? throw new ArgumentNullException(nameof(modbusService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            RefreshTagDefinitions();
        }

        public async Task<TagReadResult> ReadTagAsync(string tagName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return new TagReadResult 
                { 
                    Success = false, 
                    Error = "Tag adý boþ olamaz", 
                    TagName = tagName, 
                    Timestamp = DateTime.Now 
                };
            }

            try
            {
                // Tag tanýmýný bul
                if (!_tagDefinitions.TryGetValue(tagName, out var tagDef))
                {
                    // Cache'i yenile ve tekrar dene
                    RefreshTagDefinitions();
                    if (!_tagDefinitions.TryGetValue(tagName, out tagDef))
                    {
                        return new TagReadResult 
                        { 
                            Success = false, 
                            Error = $"'{tagName}' tag'i tanýmlanmamýþ", 
                            TagName = tagName, 
                            Timestamp = DateTime.Now 
                        };
                    }
                }

                // Address kontrolü
                if (string.IsNullOrWhiteSpace(tagDef.Address))
                {
                    return new TagReadResult
                    {
                        Success = false,
                        Error = $"'{tagName}' tag'i için Address tanýmlanmamýþ!",
                        TagName = tagName,
                        Timestamp = DateTime.Now
                    };
                }

                _logger?.Debug(nameof(TagService), $"Tag okunuyor: {tagName} -> Adres: {tagDef.Address}");

                // PLC'den oku
                var modbusResult = await _modbusService.ReadAutoAsync(tagDef.Address, ct).ConfigureAwait(false);
                
                var result = new TagReadResult
                {
                    Success = modbusResult.Success,
                    Error = modbusResult.Error,
                    Value = modbusResult.Value,
                    TagName = tagName,
                    Timestamp = DateTime.Now
                };

                // Cache'e kaydet
                if (result.Success)
                {
                    _cache.AddOrUpdate(tagName, result, (key, oldValue) => result);
                }

                // Event'i tetikle
                TagUpdated?.Invoke(this, new TagUpdatedEventArgs { TagName = tagName, Result = result });

                if (result.Success)
                {
                    _logger?.Debug(nameof(TagService), $"Tag okundu: {tagName} = {result.Value}");
                }
                else
                {
                    _logger?.Warn(nameof(TagService), $"Tag okuma hatasý: {tagName} - {result.Error}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.Error(nameof(TagService), $"Tag okuma exception: {tagName}", ex);
                return new TagReadResult 
                { 
                    Success = false, 
                    Error = ex.Message, 
                    TagName = tagName, 
                    Timestamp = DateTime.Now 
                };
            }
        }

        public async Task<bool> WriteTagAsync(string tagName, object value, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return false;

            try
            {
                // Tag tanýmýný bul
                if (!_tagDefinitions.TryGetValue(tagName, out var tagDef))
                {
                    RefreshTagDefinitions();
                    if (!_tagDefinitions.TryGetValue(tagName, out tagDef))
                    {
                        _logger?.Warn(nameof(TagService), $"Yazýlmaya çalýþýlan tag tanýmlanmamýþ: {tagName}");
                        return false;
                    }
                }

                if (tagDef.ReadOnly)
                {
                    _logger?.Warn(nameof(TagService), $"ReadOnly tag'e yazma denemesi: {tagName}");
                    return false;
                }

                // Address kontrolü
                if (string.IsNullOrWhiteSpace(tagDef.Address))
                {
                    _logger?.Error(nameof(TagService), $"'{tagName}' tag'i için Address tanýmlanmamýþ!");
                    return false;
                }

                _logger?.Debug(nameof(TagService), $"Tag yazýlýyor: {tagName} -> Adres: {tagDef.Address} = {value}");

                var success = await _modbusService.WriteAutoAsync(tagDef.Address, value, ct).ConfigureAwait(false);

                if (success)
                {
                    // Cache'i güncelle
                    var result = new TagReadResult
                    {
                        Success = true,
                        Value = value,
                        TagName = tagName,
                        Timestamp = DateTime.Now
                    };
                    
                    _cache.AddOrUpdate(tagName, result, (key, oldValue) => result);
                    
                    // Event'i tetikle
                    TagUpdated?.Invoke(this, new TagUpdatedEventArgs { TagName = tagName, Result = result });
                    
                    _logger?.Debug(nameof(TagService), $"Tag yazýldý: {tagName} = {value}");
                }
                else
                {
                    _logger?.Warn(nameof(TagService), $"Tag yazma baþarýsýz: {tagName}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger?.Error(nameof(TagService), $"Tag yazma hatasý: {tagName}", ex);
                return false;
            }
        }

        public async Task<bool> WriteTagTextAsync(string tagName, string valueText, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return false;

            try
            {
                // Tag tanýmýný bul
                if (!_tagDefinitions.TryGetValue(tagName, out var tagDef))
                {
                    RefreshTagDefinitions();
                    if (!_tagDefinitions.TryGetValue(tagName, out tagDef))
                    {
                        _logger?.Warn(nameof(TagService), $"Yazýlmaya çalýþýlan tag tanýmlanmamýþ: {tagName}");
                        return false;
                    }
                }

                // Address kontrolü
                if (string.IsNullOrWhiteSpace(tagDef.Address))
                {
                    _logger?.Error(nameof(TagService), $"'{tagName}' tag'i için Address tanýmlanmamýþ!");
                    return false;
                }

                _logger?.Debug(nameof(TagService), $"Tag metin yazýlýyor: {tagName} -> Adres: {tagDef.Address} = {valueText}");

                var success = await _modbusService.WriteTextAsync(tagDef.Address, valueText, ct).ConfigureAwait(false);

                if (success)
                {
                    _logger?.Debug(nameof(TagService), $"Tag metin olarak yazýldý: {tagName} = {valueText}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger?.Error(nameof(TagService), $"Tag metin yazma hatasý: {tagName}", ex);
                return false;
            }
        }

        public async Task<Dictionary<string, TagReadResult>> ReadTagsAsync(IEnumerable<string> tagNames, CancellationToken ct = default)
        {
            var results = new Dictionary<string, TagReadResult>();
            
            if (tagNames == null)
                return results;

            var tasks = tagNames.Select(async tagName =>
            {
                var result = await ReadTagAsync(tagName, ct).ConfigureAwait(false);
                return new { TagName = tagName, Result = result };
            });

            var completed = await Task.WhenAll(tasks).ConfigureAwait(false);
            
            foreach (var item in completed)
            {
                results[item.TagName] = item.Result;
            }

            return results;
        }

        public List<TagDefinition> GetAllTags()
        {
            RefreshTagDefinitions();
            // TagEntity -> TagDefinition dönüþümü (backward compatibility için)
            return _tagDefinitions.Values.Select(MapToLegacyDefinition).ToList();
        }

        public List<TagDefinition> GetTagsByGroup(string groupName)
        {
            RefreshTagDefinitions();
            return _tagDefinitions.Values
                .Where(t => string.Equals(t.GroupName, groupName, StringComparison.OrdinalIgnoreCase))
                .Select(MapToLegacyDefinition)
                .ToList();
        }

        public TagReadResult GetCachedValue(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return null;

            return _cache.TryGetValue(tagName, out var result) ? result : null;
        }

        public void SetCachedValue(string tagName, object value, DateTime timestamp)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return;

            var result = new TagReadResult
            {
                Success = true,
                Value = value,
                TagName = tagName,
                Timestamp = timestamp
            };

            _cache.AddOrUpdate(tagName, result, (key, oldValue) => result);
            
            // Event'i tetikle
            TagUpdated?.Invoke(this, new TagUpdatedEventArgs { TagName = tagName, Result = result });
        }

        public void ClearCache()
        {
            _cache.Clear();
            _logger?.Info(nameof(TagService), "Tag cache temizlendi");
        }

        private void RefreshTagDefinitions()
        {
            try
            {
                var allTags = _tagRepository.GetAll();
                
                _tagDefinitions.Clear();
                
                foreach (var tagDef in allTags)
                {
                    _tagDefinitions.TryAdd(tagDef.Name, tagDef);
                }

                _logger?.Debug(nameof(TagService), $"Tag tanýmlarý yenilendi: {_tagDefinitions.Count} adet");
            }
            catch (Exception ex)
            {
                _logger?.Error(nameof(TagService), "Tag tanýmlarý yenilenirken hata", ex);
            }
        }
        
        /// <summary>
        /// TagEntity -> TagDefinition (backward compatibility için)
        /// </summary>
        private TagDefinition MapToLegacyDefinition(TagEntity entity)
        {
            return new TagDefinition
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                TypeOverride = entity.DataType,
                GroupName = entity.GroupName,
                Description = entity.Description,
                PollMs = entity.PollMs,
                ReadOnly = entity.ReadOnly,
                Scale = entity.Scale,
                Offset = entity.Offset
            };
        }
    }
}