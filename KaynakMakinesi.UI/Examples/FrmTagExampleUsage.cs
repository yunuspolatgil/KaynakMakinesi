using DevExpress.XtraEditors;
using KaynakMakinesi.Core.Tags;
using KaynakMakinesi.UI.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KaynakMakinesi.UI.Examples
{
    /// <summary>
    /// TagHelper sisteminin nasýl kullanýlacaðýný gösteren örnek form
    /// </summary>
    public partial class FrmTagExampleUsage : XtraForm
    {
        public FrmTagExampleUsage()
        {
            InitializeComponent();
        }

        private void FrmTagExampleUsage_Load(object sender, EventArgs e)
        {
            // Form yüklendiðinde örnek tag'larý listele
            LoadAvailableTags();
        }

        private void LoadAvailableTags()
        {
            try
            {
                // Tüm tag'larý al
                var allTags = TagHelper.GetAllTags();
                
                // Listbox'a ekle (örnek için)
                listBoxTags.Items.Clear();
                foreach (var tag in allTags)
                {
                    listBoxTags.Items.Add($"{tag.Name} ({tag.TypeOverride}) - {tag.Description}");
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Tag'lar yüklenirken hata: {ex.Message}", "Hata");
            }
        }

        // TEK TAG OKUMA ÖRNEÐÝ
        private void btnReadSingleTag_Click(object sender, EventArgs e)
        {
            var tagName = txtTagName.Text?.Trim();
            if (string.IsNullOrEmpty(tagName))
            {
                XtraMessageBox.Show("Tag adý girin!", "Uyarý");
                return;
            }

            // Asenkron tag okuma - UI thread'e otomatik marshal edilir
            TagHelper.ReadTag(this, tagName, (result) =>
            {
                if (result.Success)
                {
                    txtValue.Text = result.ValueAsString;
                    lblStatus.Text = $"Baþarýlý - {result.Timestamp:HH:mm:ss}";
                }
                else
                {
                    lblStatus.Text = $"Hata: {result.Error}";
                }
            });
        }

        // TEK TAG YAZMA ÖRNEÐÝ  
        private void btnWriteSingleTag_Click(object sender, EventArgs e)
        {
            var tagName = txtTagName.Text?.Trim();
            var valueText = txtValue.Text?.Trim();
            
            if (string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(valueText))
            {
                XtraMessageBox.Show("Tag adý ve deðer girin!", "Uyarý");
                return;
            }

            // Asenkron tag yazma
            TagHelper.WriteTagText(this, tagName, valueText, (success) =>
            {
                if (success)
                {
                    lblStatus.Text = $"Yazma baþarýlý - {DateTime.Now:HH:mm:ss}";
                }
                else
                {
                    lblStatus.Text = "Yazma baþarýsýz!";
                }
            });
        }

        // TOPLU TAG OKUMA ÖRNEÐÝ
        private void btnReadMultipleTags_Click(object sender, EventArgs e)
        {
            // Örnek olarak belirli tag'larý oku
            var tagNames = new[] { "RampaHizlanma", "RampaYavaslama", "Olculen_Pozisyon" };
            
            TagHelper.ReadTags(this, tagNames, (results) =>
            {
                richTextBoxResults.Text = "=== TOPLU OKUMA SONUÇLARI ===\n";
                
                foreach (var kvp in results)
                {
                    var tagName = kvp.Key;
                    var result = kvp.Value;
                    
                    if (result.Success)
                    {
                        richTextBoxResults.Text += $"? {tagName}: {result.ValueAsString}\n";
                    }
                    else
                    {
                        richTextBoxResults.Text += $"? {tagName}: HATA - {result.Error}\n";
                    }
                }
                
                lblStatus.Text = $"Toplu okuma tamamlandý - {DateTime.Now:HH:mm:ss}";
            });
        }

        // CACHE'DEN HIZLI OKUMA ÖRNEÐÝ
        private void btnReadFromCache_Click(object sender, EventArgs e)
        {
            var tagName = txtTagName.Text?.Trim();
            if (string.IsNullOrEmpty(tagName))
            {
                XtraMessageBox.Show("Tag adý girin!", "Uyarý");
                return;
            }

            // Cache'den oku (PLC'ye gitmez, çok hýzlý)
            var cachedResult = TagHelper.GetCachedValue(tagName);
            
            if (cachedResult != null && cachedResult.Success)
            {
                txtValue.Text = cachedResult.ValueAsString;
                lblStatus.Text = $"Cache'den okundu - {cachedResult.Timestamp:HH:mm:ss}";
            }
            else
            {
                lblStatus.Text = "Cache'de bulunamadý veya hatalý";
            }
        }

        // GRUP BAZLI TAG LÝSTELEME ÖRNEÐÝ
        private void btnListTagsByGroup_Click(object sender, EventArgs e)
        {
            var groupName = txtGroupName.Text?.Trim();
            if (string.IsNullOrEmpty(groupName))
            {
                XtraMessageBox.Show("Grup adý girin!", "Uyarý");
                return;
            }

            var groupTags = TagHelper.GetTagsByGroup(groupName);
            
            richTextBoxResults.Text = $"=== '{groupName}' GRUBUNA AÝT TAG'LAR ===\n";
            
            if (groupTags.Count == 0)
            {
                richTextBoxResults.Text += "Bu grupta tag bulunamadý.\n";
            }
            else
            {
                foreach (var tag in groupTags)
                {
                    richTextBoxResults.Text += $"• {tag.Name} ({tag.TypeOverride}) - {tag.Description}\n";
                }
            }
        }

        // REAL-TIME TAG ÝZLEME ÖRNEÐÝ
        private void btnStartTagMonitoring_Click(object sender, EventArgs e)
        {
            // Tag güncellemelerini dinlemeye baþla
            TagHelper.SubscribeToTagUpdates(OnTagUpdated);
            
            btnStartTagMonitoring.Enabled = false;
            btnStopTagMonitoring.Enabled = true;
            
            lblStatus.Text = "Tag izleme baþlatýldý";
        }

        private void btnStopTagMonitoring_Click(object sender, EventArgs e)
        {
            // Tag güncelleme dinlemeyi durdur
            TagHelper.UnsubscribeFromTagUpdates(OnTagUpdated);
            
            btnStartTagMonitoring.Enabled = true;
            btnStopTagMonitoring.Enabled = false;
            
            lblStatus.Text = "Tag izleme durduruldu";
        }

        private void OnTagUpdated(object sender, TagUpdatedEventArgs e)
        {
            // Bu method, herhangi bir tag güncellendiðinde çaðrýlýr
            // UI thread'de çalýþýr (otomatik marshal edilir)
            
            if (InvokeRequired)
            {
                BeginInvoke((Action<object, TagUpdatedEventArgs>)OnTagUpdated, sender, e);
                return;
            }
            
            // Örnek: sadece txtTagName'deki tag'ý izle
            if (e.TagName == txtTagName.Text?.Trim())
            {
                if (e.Result.Success)
                {
                    txtValue.Text = e.Result.ValueAsString;
                    lblStatus.Text = $"Otomatik güncellendi - {e.Result.Timestamp:HH:mm:ss}";
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Form kapanýrken event listener'ý temizle
            TagHelper.UnsubscribeFromTagUpdates(OnTagUpdated);
            base.OnFormClosed(e);
        }

        #region Designer Code (Örnek UI elemanlarý)
        
        private TextEdit txtTagName;
        private TextEdit txtValue;
        private TextEdit txtGroupName;
        private SimpleButton btnReadSingleTag;
        private SimpleButton btnWriteSingleTag;
        private SimpleButton btnReadMultipleTags;
        private SimpleButton btnReadFromCache;
        private SimpleButton btnListTagsByGroup;
        private SimpleButton btnStartTagMonitoring;
        private SimpleButton btnStopTagMonitoring;
        private LabelControl lblStatus;
        private ListBoxControl listBoxTags;
        private System.Windows.Forms.RichTextBox richTextBoxResults;

        private void InitializeComponent()
        {
            this.txtTagName = new DevExpress.XtraEditors.TextEdit();
            this.txtValue = new DevExpress.XtraEditors.TextEdit();
            this.txtGroupName = new DevExpress.XtraEditors.TextEdit();
            this.btnReadSingleTag = new DevExpress.XtraEditors.SimpleButton();
            this.btnWriteSingleTag = new DevExpress.XtraEditors.SimpleButton();
            this.btnReadMultipleTags = new DevExpress.XtraEditors.SimpleButton();
            this.btnReadFromCache = new DevExpress.XtraEditors.SimpleButton();
            this.btnListTagsByGroup = new DevExpress.XtraEditors.SimpleButton();
            this.btnStartTagMonitoring = new DevExpress.XtraEditors.SimpleButton();
            this.btnStopTagMonitoring = new DevExpress.XtraEditors.SimpleButton();
            this.lblStatus = new DevExpress.XtraEditors.LabelControl();
            this.listBoxTags = new DevExpress.XtraEditors.ListBoxControl();
            this.richTextBoxResults = new System.Windows.Forms.RichTextBox();
            
            ((System.ComponentModel.ISupportInitialize)(this.txtTagName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGroupName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.listBoxTags)).BeginInit();
            this.SuspendLayout();
            
            // Form
            this.Text = "Tag Kullaným Örnekleri";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Controls setup - basit yerleþim
            this.txtTagName.Location = new System.Drawing.Point(12, 12);
            this.txtTagName.Size = new System.Drawing.Size(200, 20);
            
            this.txtValue.Location = new System.Drawing.Point(220, 12);
            this.txtValue.Size = new System.Drawing.Size(150, 20);
            
            this.btnReadSingleTag.Location = new System.Drawing.Point(12, 40);
            this.btnReadSingleTag.Size = new System.Drawing.Size(100, 23);
            this.btnReadSingleTag.Text = "Tag Oku";
            this.btnReadSingleTag.Click += btnReadSingleTag_Click;
            
            this.btnWriteSingleTag.Location = new System.Drawing.Point(120, 40);
            this.btnWriteSingleTag.Size = new System.Drawing.Size(100, 23);
            this.btnWriteSingleTag.Text = "Tag Yaz";
            this.btnWriteSingleTag.Click += btnWriteSingleTag_Click;
            
            this.lblStatus.Location = new System.Drawing.Point(12, 70);
            this.lblStatus.Size = new System.Drawing.Size(400, 13);
            this.lblStatus.Text = "Hazýr";
            
            // Add controls
            this.Controls.Add(this.txtTagName);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.btnReadSingleTag);
            this.Controls.Add(this.btnWriteSingleTag);
            this.Controls.Add(this.lblStatus);
            
            ((System.ComponentModel.ISupportInitialize)(this.txtTagName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGroupName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.listBoxTags)).EndInit();
            this.ResumeLayout(false);
        }
        
        #endregion
    }
}