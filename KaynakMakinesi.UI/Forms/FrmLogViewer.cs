using System;
using System.Data.SQLite;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace KaynakMakinesi.UI.Forms
{
    public class FrmLogViewer : XtraForm
    {
        private MemoEdit txtLogs;
        private SimpleButton btnRefresh;
        private SimpleButton btnClear;
        private SimpleButton btnClose;
        private SimpleButton btnCopyToClipboard;
        private LabelControl lblInfo;
        
        private readonly string _dbPath;
        
        public FrmLogViewer(string dbPath)
        {
            _dbPath = dbPath;
            InitializeComponent();
            LoadLogs();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Log Görüntüleyici";
            this.Width = 1000;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterParent;
            
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };
            
            btnRefresh = new SimpleButton
            {
                Text = "Yenile",
                Width = 100,
                Height = 35,
                Location = new System.Drawing.Point(10, 7)
            };
            btnRefresh.Click += (s, e) => LoadLogs();
            panel.Controls.Add(btnRefresh);
            
            btnCopyToClipboard = new SimpleButton
            {
                Text = "Panoya Kopyala",
                Width = 120,
                Height = 35,
                Location = new System.Drawing.Point(120, 7)
            };
            btnCopyToClipboard.Click += BtnCopyToClipboard_Click;
            panel.Controls.Add(btnCopyToClipboard);
            
            btnClear = new SimpleButton
            {
                Text = "Temizle",
                Width = 100,
                Height = 35,
                Location = new System.Drawing.Point(250, 7)
            };
            btnClear.Click += BtnClear_Click;
            panel.Controls.Add(btnClear);
            
            btnClose = new SimpleButton
            {
                Text = "Kapat",
                Width = 100,
                Height = 35,
                Location = new System.Drawing.Point(360, 7)
            };
            btnClose.Click += (s, e) => Close();
            panel.Controls.Add(btnClose);
            
            lblInfo = new LabelControl
            {
                Text = "Yükleniyor...",
                Location = new System.Drawing.Point(470, 15),
                AutoSizeMode = LabelAutoSizeMode.None,
                Width = 400
            };
            panel.Controls.Add(lblInfo);
            
            this.Controls.Add(panel);
            
            txtLogs = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Both,
                    WordWrap = false
                }
            };
            this.Controls.Add(txtLogs);
        }
        
        private void LoadLogs()
        {
            try
            {
                var logs = new System.Text.StringBuilder();
                int count = 0;
                
                using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    conn.Open();
                    
                    // Son 1000 log'u getir
                    var cmd = new SQLiteCommand(@"
                        SELECT Timestamp, Level, Source, Message, Exception 
                        FROM LogEntry 
                        ORDER BY Id DESC 
                        LIMIT 1000", conn);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            count++;
                            var timestamp = reader.GetString(0);
                            var level = reader.GetString(1);
                            var source = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            var message = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            var exception = reader.IsDBNull(4) ? "" : reader.GetString(4);
                            
                            logs.AppendLine($"[{timestamp}] [{level}] {source} - {message}");
                            
                            if (!string.IsNullOrEmpty(exception))
                            {
                                logs.AppendLine($"  Exception: {exception}");
                            }
                        }
                    }
                }
                
                txtLogs.Text = logs.ToString();
                lblInfo.Text = $"{count} adet log yüklendi";
                
                // En sona scroll
                txtLogs.SelectionStart = txtLogs.Text.Length;
                txtLogs.ScrollToCaret();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Log yükleme hatasý:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnCopyToClipboard_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtLogs.Text))
                {
                    Clipboard.SetText(txtLogs.Text);
                    XtraMessageBox.Show("Log'lar panoya kopyalandý!", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Panoya kopyalama hatasý:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnClear_Click(object sender, EventArgs e)
        {
            try
            {
                var result = XtraMessageBox.Show(
                    "TÜM LOG'LAR SÝLÝNECEK!\n\nEmin misiniz?",
                    "Onay",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                
                if (result != DialogResult.Yes)
                    return;
                
                using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("DELETE FROM LogEntry", conn);
                    cmd.ExecuteNonQuery();
                }
                
                LoadLogs();
                XtraMessageBox.Show("Tüm log'lar silindi.", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Log silme hatasý:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
