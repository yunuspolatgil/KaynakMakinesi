using KaynakMakinesi.Core.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaynakMakinesi.UI.Utils
{
    /// <summary>
    /// Formlarda tag iþlemlerini kolaylaþtýran yardýmcý sýnýf
    /// </summary>
    public static class TagHelper
    {
        private static ITagService _tagService;

        /// <summary>
        /// TagService'i baþlat (Program.cs'den çaðrýlýr)
        /// </summary>
        public static void Initialize(ITagService tagService)
        {
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        }

        /// <summary>
        /// Tag oku ve sonucu Action ile döndür (async UI update için)
        /// </summary>
        public static async void ReadTag(Control control, string tagName, Action<TagReadResult> onResult)
        {
            if (_tagService == null)
            {
                onResult?.Invoke(new TagReadResult 
                { 
                    Success = false, 
                    Error = "TagService baþlatýlmamýþ", 
                    TagName = tagName 
                });
                return;
            }

            try
            {
                var result = await _tagService.ReadTagAsync(tagName, CancellationToken.None).ConfigureAwait(false);
                
                if (control.InvokeRequired)
                {
                    control.BeginInvoke((Action)(() => onResult?.Invoke(result)));
                }
                else
                {
                    onResult?.Invoke(result);
                }
            }
            catch (Exception ex)
            {
                var errorResult = new TagReadResult 
                { 
                    Success = false, 
                    Error = ex.Message, 
                    TagName = tagName, 
                    Timestamp = DateTime.Now 
                };

                if (control.InvokeRequired)
                {
                    control.BeginInvoke((Action)(() => onResult?.Invoke(errorResult)));
                }
                else
                {
                    onResult?.Invoke(errorResult);
                }
            }
        }

        /// <summary>
        /// Tag yaz ve sonucu Action ile döndür
        /// </summary>
        public static async void WriteTag(Control control, string tagName, object value, Action<bool> onResult = null)
        {
            if (_tagService == null)
            {
                onResult?.Invoke(false);
                return;
            }

            try
            {
                var success = await _tagService.WriteTagAsync(tagName, value, CancellationToken.None).ConfigureAwait(false);
                
                if (control.InvokeRequired)
                {
                    control.BeginInvoke((Action)(() => onResult?.Invoke(success)));
                }
                else
                {
                    onResult?.Invoke(success);
                }
            }
            catch (Exception)
            {
                if (control.InvokeRequired)
                {
                    control.BeginInvoke((Action)(() => onResult?.Invoke(false)));
                }
                else
                {
                    onResult?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// Tag yaz (metin olarak) ve sonucu Action ile döndür
        /// </summary>
        public static async void WriteTagText(Control control, string tagName, string valueText, Action<bool> onResult = null)
        {
            if (_tagService == null)
            {
                onResult?.Invoke(false);
                return;
            }

            try
            {
                var success = await _tagService.WriteTagTextAsync(tagName, valueText, CancellationToken.None).ConfigureAwait(false);
                
                if (control.InvokeRequired)
                {
                    control.BeginInvoke((Action)(() => onResult?.Invoke(success)));
                }
                else
                {
                    onResult?.Invoke(success);
                }
            }
            catch (Exception)
            {
                if (control.InvokeRequired)
                {
                    control.BeginInvoke((Action)(() => onResult?.Invoke(false)));
                }
                else
                {
                    onResult?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// Birden fazla tag'i toplu oku
        /// </summary>
        public static async void ReadTags(Control control, IEnumerable<string> tagNames, Action<Dictionary<string, TagReadResult>> onResult)
        {
            if (_tagService == null)
            {
                onResult?.Invoke(new Dictionary<string, TagReadResult>());
                return;
            }

            try
            {
                var results = await _tagService.ReadTagsAsync(tagNames, CancellationToken.None).ConfigureAwait(false);
                
                if (control.InvokeRequired)
                {
                    control.BeginInvoke((Action)(() => onResult?.Invoke(results)));
                }
                else
                {
                    onResult?.Invoke(results);
                }
            }
            catch (Exception)
            {
                if (control.InvokeRequired)
                {
                    control.BeginInvoke((Action)(() => onResult?.Invoke(new Dictionary<string, TagReadResult>())));
                }
                else
                {
                    onResult?.Invoke(new Dictionary<string, TagReadResult>());
                }
            }
        }

        /// <summary>
        /// Cache'den deðer al (PLC'ye gitmez, hýzlýdýr)
        /// </summary>
        public static TagReadResult GetCachedValue(string tagName)
        {
            return _tagService?.GetCachedValue(tagName);
        }

        /// <summary>
        /// Tüm tag'larý listele
        /// </summary>
        public static List<TagDefinition> GetAllTags()
        {
            return _tagService?.GetAllTags() ?? new List<TagDefinition>();
        }

        /// <summary>
        /// Gruba göre tag'larý listele
        /// </summary>
        public static List<TagDefinition> GetTagsByGroup(string groupName)
        {
            return _tagService?.GetTagsByGroup(groupName) ?? new List<TagDefinition>();
        }

        /// <summary>
        /// Tag güncellemelerini dinle
        /// </summary>
        public static void SubscribeToTagUpdates(EventHandler<TagUpdatedEventArgs> handler)
        {
            if (_tagService != null)
            {
                _tagService.TagUpdated += handler;
            }
        }

        /// <summary>
        /// Tag güncelleme dinlemeyi durdur
        /// </summary>
        public static void UnsubscribeFromTagUpdates(EventHandler<TagUpdatedEventArgs> handler)
        {
            if (_tagService != null)
            {
                _tagService.TagUpdated -= handler;
            }
        }
    }
}