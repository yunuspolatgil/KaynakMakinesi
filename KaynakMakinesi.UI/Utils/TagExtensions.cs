using KaynakMakinesi.Core.Tags;
using KaynakMakinesi.UI.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaynakMakinesi.UI.Utils
{
    /// <summary>
    /// Tag iþlemleri için pratik extension metodlarý
    /// </summary>
    public static class TagExtensions
    {
        /// <summary>
        /// TextBox'a tag deðerini asenkron yükle
        /// </summary>
        public static void LoadFromTag(this TextBox textBox, string tagName)
        {
            TagHelper.ReadTag(textBox, tagName, (result) =>
            {
                if (result.Success)
                {
                    textBox.Text = result.ValueAsString;
                }
            });
        }

        /// <summary>
        /// TextBox deðerini tag'e asenkron kaydet
        /// </summary>
        public static void SaveToTag(this TextBox textBox, string tagName, Action<bool> onComplete = null)
        {
            TagHelper.WriteTagText(textBox, tagName, textBox.Text, onComplete);
        }

        /// <summary>
        /// DevExpress TextEdit'e tag deðerini asenkron yükle
        /// </summary>
        public static void LoadFromTag(this DevExpress.XtraEditors.TextEdit textEdit, string tagName)
        {
            TagHelper.ReadTag(textEdit, tagName, (result) =>
            {
                if (result.Success)
                {
                    textEdit.Text = result.ValueAsString;
                }
            });
        }

        /// <summary>
        /// DevExpress TextEdit deðerini tag'e asenkron kaydet
        /// </summary>
        public static void SaveToTag(this DevExpress.XtraEditors.TextEdit textEdit, string tagName, Action<bool> onComplete = null)
        {
            TagHelper.WriteTagText(textEdit, tagName, textEdit.Text, onComplete);
        }

        /// <summary>
        /// Label'a tag deðerini asenkron yükle
        /// </summary>
        public static void LoadFromTag(this Label label, string tagName)
        {
            TagHelper.ReadTag(label, tagName, (result) =>
            {
                if (result.Success)
                {
                    label.Text = result.ValueAsString;
                }
                else
                {
                    label.Text = "---";
                }
            });
        }
    }

    /// <summary>
    /// Sýk kullanýlan tag iþlemleri için kýsayol metodlar
    /// </summary>
    public static class QuickTags
    {
        /// <summary>
        /// Synchronous tag okuma (sadece test/debug için, UI'da kullanma!)
        /// </summary>
        public static string ReadTagSync(string tagName, string defaultValue = "")
        {
            var cached = TagHelper.GetCachedValue(tagName);
            return cached?.Success == true ? cached.ValueAsString : defaultValue;
        }

        /// <summary>
        /// Boolean tag'i oku
        /// </summary>
        public static void ReadBoolTag(Control control, string tagName, Action<bool> onResult)
        {
            TagHelper.ReadTag(control, tagName, (result) =>
            {
                if (result.Success && result.Value != null)
                {
                    if (bool.TryParse(result.ValueAsString, out var boolValue))
                    {
                        onResult?.Invoke(boolValue);
                    }
                    else if (int.TryParse(result.ValueAsString, out var intValue))
                    {
                        onResult?.Invoke(intValue != 0);
                    }
                }
                else
                {
                    onResult?.Invoke(false);
                }
            });
        }

        /// <summary>
        /// Integer tag'i oku
        /// </summary>
        public static void ReadIntTag(Control control, string tagName, Action<int> onResult)
        {
            TagHelper.ReadTag(control, tagName, (result) =>
            {
                if (result.Success && int.TryParse(result.ValueAsString, out var intValue))
                {
                    onResult?.Invoke(intValue);
                }
                else
                {
                    onResult?.Invoke(0);
                }
            });
        }

        /// <summary>
        /// Float tag'i oku
        /// </summary>
        public static void ReadFloatTag(Control control, string tagName, Action<float> onResult)
        {
            TagHelper.ReadTag(control, tagName, (result) =>
            {
                if (result.Success && float.TryParse(result.ValueAsString, out var floatValue))
                {
                    onResult?.Invoke(floatValue);
                }
                else
                {
                    onResult?.Invoke(0f);
                }
            });
        }

        /// <summary>
        /// Boolean deðeri tag'e yaz
        /// </summary>
        public static void WriteBoolTag(Control control, string tagName, bool value, Action<bool> onComplete = null)
        {
            TagHelper.WriteTag(control, tagName, value, onComplete);
        }

        /// <summary>
        /// Integer deðeri tag'e yaz
        /// </summary>
        public static void WriteIntTag(Control control, string tagName, int value, Action<bool> onComplete = null)
        {
            TagHelper.WriteTag(control, tagName, value, onComplete);
        }

        /// <summary>
        /// Float deðeri tag'e yaz
        /// </summary>
        public static void WriteFloatTag(Control control, string tagName, float value, Action<bool> onComplete = null)
        {
            TagHelper.WriteTag(control, tagName, value, onComplete);
        }

        /// <summary>
        /// Birden fazla tag'i tek seferde oku ve dictionary olarak döndür
        /// </summary>
        public static void ReadTagGroup(Control control, Dictionary<string, string> tagMappings, Action<Dictionary<string, string>> onResult)
        {
            var tagNames = new List<string>(tagMappings.Keys);
            
            TagHelper.ReadTags(control, tagNames, (results) =>
            {
                var mappedResults = new Dictionary<string, string>();
                
                foreach (var kvp in tagMappings)
                {
                    var tagName = kvp.Key;
                    var displayName = kvp.Value;
                    
                    if (results.TryGetValue(tagName, out var result) && result.Success)
                    {
                        mappedResults[displayName] = result.ValueAsString;
                    }
                    else
                    {
                        mappedResults[displayName] = "---";
                    }
                }
                
                onResult?.Invoke(mappedResults);
            });
        }

        /// <summary>
        /// Tag deðiþikliklerini otomatik dinle ve action'ý tetikle
        /// </summary>
        public static void WatchTag(string tagName, Action<TagReadResult> onChange)
        {
            TagHelper.SubscribeToTagUpdates((sender, e) =>
            {
                if (e.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                {
                    onChange?.Invoke(e.Result);
                }
            });
        }
    }

    /// <summary>
    /// Form düzeyinde tag operasyonlarýný kolaylaþtýran base class
    /// </summary>
    public class TagEnabledForm : Form
    {
        private readonly List<EventHandler<TagUpdatedEventArgs>> _tagHandlers = new List<EventHandler<TagUpdatedEventArgs>>();

        /// <summary>
        /// Form için tag okuma
        /// </summary>
        protected void ReadTag(string tagName, Action<TagReadResult> onResult)
        {
            TagHelper.ReadTag(this, tagName, onResult);
        }

        /// <summary>
        /// Form için tag yazma
        /// </summary>
        protected void WriteTag(string tagName, object value, Action<bool> onResult = null)
        {
            TagHelper.WriteTag(this, tagName, value, onResult);
        }

        /// <summary>
        /// Form için toplu tag okuma
        /// </summary>
        protected void ReadTags(IEnumerable<string> tagNames, Action<Dictionary<string, TagReadResult>> onResult)
        {
            TagHelper.ReadTags(this, tagNames, onResult);
        }

        /// <summary>
        /// Tag deðiþikliklerini dinle (form kapanýrken otomatik temizlenir)
        /// </summary>
        protected void WatchTag(string tagName, Action<TagReadResult> onChange)
        {
            EventHandler<TagUpdatedEventArgs> handler = (sender, e) =>
            {
                if (e.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((Action<TagReadResult>)onChange, e.Result);
                    }
                    else
                    {
                        onChange?.Invoke(e.Result);
                    }
                }
            };

            _tagHandlers.Add(handler);
            TagHelper.SubscribeToTagUpdates(handler);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Tüm tag listener'larýný temizle
            foreach (var handler in _tagHandlers)
            {
                TagHelper.UnsubscribeFromTagUpdates(handler);
            }
            _tagHandlers.Clear();

            base.OnFormClosed(e);
        }
    }
}