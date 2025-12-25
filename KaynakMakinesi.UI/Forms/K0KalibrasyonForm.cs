using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Motor;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.Core.Tags;
using KaynakMakinesi.UI.Controls;

namespace KaynakMakinesi.UI.Forms
{
    /// <summary>
    /// K0 Motor (Torç Sað) Kalibrasyon Formu
    /// </summary>
    public class K0KalibrasyonForm : BaseKalibrasyonForm
    {
        // Adým 1: Home Controls
        private GroupControl grpHome;
        private MotorCalcEdit txtHomeHiz;
        private MotorCalcEdit txtHomeYavas;
        private SimpleButton btnHomeGit;
        
        // Adým 2: Manuel Hareket Controls
        private GroupControl grpManuel;
        private MotorCalcEdit txtIleriHiz;
        private MotorCalcEdit txtIleriRampaHiz;
        private MotorCalcEdit txtIleriRampaYavas;
        private MotorCalcEdit txtGeriHiz;
        private MotorCalcEdit txtGeriRampaHiz;
        private MotorCalcEdit txtGeriRampaYavas;
        private SimpleButton btnIleri;
        private SimpleButton btnGeri;
        private SimpleButton btnDur;
        
        // Adým 3: Kalibrasyon Controls
        private GroupControl grpKalibrasyon;
        private MotorCalcEdit txtOlculenPozisyon;
        private LabelControl lblCikisPozisyonu;
        private SimpleButton btnEksenKalibrasyon;
        
        // Adým 4: Rampa Set Controls
        private GroupControl grpRampa;
        private MotorCalcEdit txtRampaHizlanma;
        private MotorCalcEdit txtRampaYavaslama;
        private SimpleButton btnRampaSet;
        
        // Adým 5: Pozisyon Test Controls
        private GroupControl grpPozisyonTest;
        private MotorCalcEdit txtTestPozisyon;
        private MotorCalcEdit txtTestHiz;
        private SimpleButton btnPozisyonaGit;
        private LabelControl lblMevcutPozisyon;
        
        // Log Panel
        private GroupControl grpLog;
        private MemoEdit txtLog;
        private SimpleButton btnOncekiKalibrasyonlar;
        
        public K0KalibrasyonForm(
            IModbusService modbusService,
            ITagService tagService,
            IAppLogger logger,
            IKalibrasyonService kalibrasyonService)
            : base(MotorConfig.Presets.K0_TorcSag, modbusService, tagService, logger, kalibrasyonService)
        {
            this.Load += K0KalibrasyonForm_Load;
        }
        
        private void K0KalibrasyonForm_Load(object sender, EventArgs e)
        {
            try
            {
                LoadInitialValues();
                _logger?.Info(GetType().Name, "Form yüklendi");
            }
            catch (Exception ex)
            {
                _logger?.Error(GetType().Name, "Form Load hatasý", ex);
                MessageBox.Show($"Form yüklenirken hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void InitializeMotorUI()
        {
            try
            {
                SuspendLayout();
                
                // Ana panel
                var mainPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    Padding = new Padding(10),
                    BackColor = Color.White
                };
                
                int yPos = 10; // Panel içinde baþlangýç
                
                // 1. HOME GRUBU
                CreateHomeGroup();
                grpHome.Location = new Point(10, yPos);
                mainPanel.Controls.Add(grpHome);
                yPos += grpHome.Height + 10;
                
                // 2. MANUEL HAREKET GRUBU
                CreateManuelHareketGroup();
                grpManuel.Location = new Point(10, yPos);
                mainPanel.Controls.Add(grpManuel);
                yPos += grpManuel.Height + 10;
                
                // 3. KALÝBRASYON GRUBU
                CreateKalibrasyonGroup();
                grpKalibrasyon.Location = new Point(10, yPos);
                mainPanel.Controls.Add(grpKalibrasyon);
                yPos += grpKalibrasyon.Height + 10;
                
                // 4. RAMPA SET GRUBU
                CreateRampaGroup();
                grpRampa.Location = new Point(10, yPos);
                mainPanel.Controls.Add(grpRampa);
                yPos += grpRampa.Height + 10;
                
                // 5. POZÝSYON TEST GRUBU
                CreatePozisyonTestGroup();
                grpPozisyonTest.Location = new Point(10, yPos);
                mainPanel.Controls.Add(grpPozisyonTest);
                yPos += grpPozisyonTest.Height + 10;
                
                // 6. LOG GRUBU
                CreateLogGroup();
                grpLog.Location = new Point(10, yPos);
                mainPanel.Controls.Add(grpLog);
                
                // MainPanel'i form'a ekle
                this.Controls.Add(mainPanel);
                mainPanel.BringToFront(); // En öne getir
                
                ResumeLayout();
                
                _logger?.Info(GetType().Name, $"Motor UI baþarýyla oluþturuldu. Panel control count: {mainPanel.Controls.Count}");
            }
            catch (Exception ex)
            {
                _logger?.Error(GetType().Name, "InitializeMotorUI hatasý", ex);
                MessageBox.Show($"Motor UI oluþturulurken hata:\n{ex.Message}\n\nDetay:\n{ex.StackTrace}", 
                    "UI Hatasý", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                throw;
            }
        }

        #region UI Creation Methods

        private void CreateHomeGroup()
        {
            grpHome = new GroupControl
            {
                Text = "ADIM 1: Home Pozisyonu",
                Width = 860,
                Height = 120
            };
            
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 6,
                RowCount = 2
            };
            
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            
            // Row 0
            panel.Controls.Add(new LabelControl { Text = "Home Hýz:", Dock = DockStyle.Fill }, 0, 0);
            txtHomeHiz = new MotorCalcEdit 
            { 
                PromptTitle = "Home Hýz (mm/min)",
                EnableValidation = true,
                MinValue = _motorConfig.MinHiz,
                MaxValue = _motorConfig.MaxHiz,
                DecimalPlaces = 0
            };
            txtHomeHiz.Dock = DockStyle.Fill;
            panel.Controls.Add(txtHomeHiz, 1, 0);
            
            panel.Controls.Add(new LabelControl { Text = "Home Yavaþ:", Dock = DockStyle.Fill }, 2, 0);
            txtHomeYavas = new MotorCalcEdit 
            { 
                PromptTitle = "Home Yavaþ (mm/min)",
                EnableValidation = true,
                MinValue = _motorConfig.MinHiz,
                MaxValue = _motorConfig.MaxHiz,
                DecimalPlaces = 0
            };
            txtHomeYavas.Dock = DockStyle.Fill;
            panel.Controls.Add(txtHomeYavas, 3, 0);
            
            btnHomeGit = new SimpleButton
            {
                Text = "Home Git",
                Dock = DockStyle.Fill,
                Appearance = { Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.LightGreen }
            };
            btnHomeGit.Click += BtnHomeGit_Click;
            panel.SetRowSpan(btnHomeGit, 2);
            panel.Controls.Add(btnHomeGit, 5, 0);
            
            // Row 1 - Açýklama
            var lblInfo = new LabelControl
            {
                Text = "?? Ýlk olarak motoru home pozisyonuna çekin. Bu, kalibrasyonun baþlangýç noktasýdýr.",
                Dock = DockStyle.Fill,
                Appearance = { ForeColor = Color.DarkBlue },
                AutoSizeMode = LabelAutoSizeMode.Vertical
            };
            panel.SetColumnSpan(lblInfo, 4);
            panel.Controls.Add(lblInfo, 0, 1);
            
            grpHome.Controls.Add(panel);
        }

        private void CreateManuelHareketGroup()
        {
            grpManuel = new GroupControl
            {
                Text = "?? ADIM 2: Manuel Hareket",
                Width = 860,
                Height = 200,
                Enabled = false
            };
            
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 6,
                RowCount = 4
            };
            
            for (int i = 0; i < 6; i++)
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66f));
            
            // Ýleri parametreleri
            panel.Controls.Add(new LabelControl { Text = "Ýleri Hýz:", Dock = DockStyle.Fill }, 0, 0);
            txtIleriHiz = CreateMotorCalcEdit("Ýleri Hýz", true, 0);
            panel.Controls.Add(txtIleriHiz, 1, 0);
            
            panel.Controls.Add(new LabelControl { Text = "Ýleri Rampa Hýz:", Dock = DockStyle.Fill }, 2, 0);
            txtIleriRampaHiz = CreateMotorCalcEdit("Ýleri Rampa Hýz", true, 0);
            panel.Controls.Add(txtIleriRampaHiz, 3, 0);
            
            panel.Controls.Add(new LabelControl { Text = "Ýleri Rampa Yavaþ:", Dock = DockStyle.Fill }, 4, 0);
            txtIleriRampaYavas = CreateMotorCalcEdit("Ýleri Rampa Yavaþ", true, 0);
            panel.Controls.Add(txtIleriRampaYavas, 5, 0);
            
            // Geri parametreleri
            panel.Controls.Add(new LabelControl { Text = "Geri Hýz:", Dock = DockStyle.Fill }, 0, 1);
            txtGeriHiz = CreateMotorCalcEdit("Geri Hýz", true, 0);
            panel.Controls.Add(txtGeriHiz, 1, 1);
            
            panel.Controls.Add(new LabelControl { Text = "Geri Rampa Hýz:", Dock = DockStyle.Fill }, 2, 1);
            txtGeriRampaHiz = CreateMotorCalcEdit("Geri Rampa Hýz", true, 0);
            panel.Controls.Add(txtGeriRampaHiz, 3, 1);
            
            panel.Controls.Add(new LabelControl { Text = "Geri Rampa Yavaþ:", Dock = DockStyle.Fill }, 4, 1);
            txtGeriRampaYavas = CreateMotorCalcEdit("Geri Rampa Yavaþ", true, 0);
            panel.Controls.Add(txtGeriRampaYavas, 5, 1);
            
            // Butonlar
            var btnPanel = new Panel { Dock = DockStyle.Fill };
            
            btnIleri = new SimpleButton
            {
                Text = "?? ÝLERÝ",
                Width = 120,
                Height = 50,
                Location = new Point(10, 5),
                Appearance = { Font = new Font("Segoe UI", 11f, FontStyle.Bold), BackColor = Color.LightBlue }
            };
            btnIleri.MouseDown += (s, e) => BtnIleri_MouseDown();
            btnIleri.MouseUp += (s, e) => BtnDur_Click(s, e);
            btnPanel.Controls.Add(btnIleri);
            
            btnDur = new SimpleButton
            {
                Text = "?? DUR",
                Width = 120,
                Height = 50,
                Location = new Point(140, 5),
                Appearance = { Font = new Font("Segoe UI", 11f, FontStyle.Bold), BackColor = Color.Orange }
            };
            btnDur.Click += BtnDur_Click;
            btnPanel.Controls.Add(btnDur);
            
            btnGeri = new SimpleButton
            {
                Text = "?? GERÝ",
                Width = 120,
                Height = 50,
                Location = new Point(270, 5),
                Appearance = { Font = new Font("Segoe UI", 11f, FontStyle.Bold), BackColor = Color.LightCoral }
            };
            btnGeri.MouseDown += (s, e) => BtnGeri_MouseDown();
            btnGeri.MouseUp += (s, e) => BtnDur_Click(s, e);
            btnPanel.Controls.Add(btnGeri);
            
            panel.SetColumnSpan(btnPanel, 6);
            panel.Controls.Add(btnPanel, 0, 2);
            
            // Açýklama
            var lblInfo = new LabelControl
            {
                Text = "?? Manuel hareket ile motoru istenen pozisyona getirin. Ölçüm yapýn ve deðeri bir sonraki adýmda girin.",
                Dock = DockStyle.Fill,
                Appearance = { ForeColor = Color.DarkBlue },
                AutoSizeMode = LabelAutoSizeMode.Vertical
            };
            panel.SetColumnSpan(lblInfo, 6);
            panel.Controls.Add(lblInfo, 0, 3);
            
            grpManuel.Controls.Add(panel);
        }

        private void CreateKalibrasyonGroup()
        {
            grpKalibrasyon = new GroupControl
            {
                Text = "?? ADIM 3: Eksen Kalibrasyonu",
                Width = 860,
                Height = 120,
                Enabled = false
            };
            
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 6,
                RowCount = 2
            };
            
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            
            // Row 0
            panel.Controls.Add(new LabelControl { Text = "Ölçülen Pozisyon:", Dock = DockStyle.Fill }, 0, 0);
            txtOlculenPozisyon = new MotorCalcEdit 
            { 
                PromptTitle = "Ölçülen Pozisyon (mm)",
                EnableValidation = true,
                MinValue = (decimal)_motorConfig.MinPozisyon,
                MaxValue = (decimal)_motorConfig.MaxPozisyon,
                DecimalPlaces = 2
            };
            txtOlculenPozisyon.Dock = DockStyle.Fill;
            panel.Controls.Add(txtOlculenPozisyon, 1, 0);
            
            panel.Controls.Add(new LabelControl { Text = "Çýkýþ Pozisyonu:", Dock = DockStyle.Fill }, 2, 0);
            lblCikisPozisyonu = new LabelControl
            {
                Text = "0.00 mm",
                Dock = DockStyle.Fill,
                Appearance = { Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.DarkGreen, TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center } }
            };
            panel.Controls.Add(lblCikisPozisyonu, 3, 0);
            
            btnEksenKalibrasyon = new SimpleButton
            {
                Text = "?? Kalibrasyon Yap",
                Dock = DockStyle.Fill,
                Appearance = { Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.LightSeaGreen }
            };
            btnEksenKalibrasyon.Click += BtnEksenKalibrasyon_Click;
            panel.SetRowSpan(btnEksenKalibrasyon, 2);
            panel.Controls.Add(btnEksenKalibrasyon, 5, 0);
            
            // Row 1
            var lblInfo = new LabelControl
            {
                Text = "?? Ölçtüðünüz mesafe deðerini girin ve 'Kalibrasyon Yap' butonuna basýn. Motor home konumuna gidecektir.",
                Dock = DockStyle.Fill,
                Appearance = { ForeColor = Color.DarkBlue },
                AutoSizeMode = LabelAutoSizeMode.Vertical
            };
            panel.SetColumnSpan(lblInfo, 4);
            panel.Controls.Add(lblInfo, 0, 1);
            
            grpKalibrasyon.Controls.Add(panel);
        }

        private void CreateRampaGroup()
        {
            grpRampa = new GroupControl
            {
                Text = "? ADIM 4: Rampa Ayarlarý",
                Width = 860,
                Height = 120,
                Enabled = false
            };
            
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 6,
                RowCount = 2
            };
            
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            
            // Row 0
            panel.Controls.Add(new LabelControl { Text = "Rampa Hýzlanma:", Dock = DockStyle.Fill }, 0, 0);
            txtRampaHizlanma = new MotorCalcEdit 
            { 
                PromptTitle = "Rampa Hýzlanma",
                EnableValidation = true,
                MinValue = (decimal)_motorConfig.MinRampa,
                MaxValue = (decimal)_motorConfig.MaxRampa,
                DecimalPlaces = 1
            };
            txtRampaHizlanma.Dock = DockStyle.Fill;
            panel.Controls.Add(txtRampaHizlanma, 1, 0);
            
            panel.Controls.Add(new LabelControl { Text = "Rampa Yavaþlama:", Dock = DockStyle.Fill }, 2, 0);
            txtRampaYavaslama = new MotorCalcEdit 
            { 
                PromptTitle = "Rampa Yavaþlama",
                EnableValidation = true,
                MinValue = (decimal)_motorConfig.MinRampa,
                MaxValue = (decimal)_motorConfig.MaxRampa,
                DecimalPlaces = 1
            };
            txtRampaYavaslama.Dock = DockStyle.Fill;
            panel.Controls.Add(txtRampaYavaslama, 3, 0);
            
            btnRampaSet = new SimpleButton
            {
                Text = "? Rampa Set",
                Dock = DockStyle.Fill,
                Appearance = { Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.LightSkyBlue }
            };
            btnRampaSet.Click += BtnRampaSet_Click;
            panel.SetRowSpan(btnRampaSet, 2);
            panel.Controls.Add(btnRampaSet, 5, 0);
            
            // Row 1
            var lblInfo = new LabelControl
            {
                Text = "?? Motor için rampa hýzlanma ve yavaþlama deðerlerini ayarlayýn.",
                Dock = DockStyle.Fill,
                Appearance = { ForeColor = Color.DarkBlue },
                AutoSizeMode = LabelAutoSizeMode.Vertical
            };
            panel.SetColumnSpan(lblInfo, 4);
            panel.Controls.Add(lblInfo, 0, 1);
            
            grpRampa.Controls.Add(panel);
        }

        private void CreatePozisyonTestGroup()
        {
            grpPozisyonTest = new GroupControl
            {
                Text = "?? ADIM 5: Pozisyon Testi",
                Width = 860,
                Height = 140,
                Enabled = false
            };
            
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 6,
                RowCount = 3
            };
            
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            
            // Row 0
            panel.Controls.Add(new LabelControl { Text = "Test Pozisyon:", Dock = DockStyle.Fill }, 0, 0);
            txtTestPozisyon = new MotorCalcEdit 
            { 
                PromptTitle = "Test Pozisyon (mm)",
                EnableValidation = true,
                MinValue = (decimal)_motorConfig.MinPozisyon,
                MaxValue = (decimal)_motorConfig.MaxPozisyon,
                DecimalPlaces = 2
            };
            txtTestPozisyon.Dock = DockStyle.Fill;
            panel.Controls.Add(txtTestPozisyon, 1, 0);
            
            panel.Controls.Add(new LabelControl { Text = "Hareket Hýzý:", Dock = DockStyle.Fill }, 2, 0);
            txtTestHiz = new MotorCalcEdit 
            { 
                PromptTitle = "Hareket Hýzý (mm/min)",
                EnableValidation = true,
                MinValue = _motorConfig.MinHiz,
                MaxValue = _motorConfig.MaxHiz,
                DecimalPlaces = 0
            };
            txtTestHiz.Dock = DockStyle.Fill;
            panel.Controls.Add(txtTestHiz, 3, 0);
            
            btnPozisyonaGit = new SimpleButton
            {
                Text = "?? Pozisyona Git",
                Dock = DockStyle.Fill,
                Appearance = { Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.LightGoldenrodYellow }
            };
            btnPozisyonaGit.Click += BtnPozisyonaGit_Click;
            panel.SetRowSpan(btnPozisyonaGit, 3);
            panel.Controls.Add(btnPozisyonaGit, 5, 0);
            
            // Row 1 - Mevcut Pozisyon
            panel.Controls.Add(new LabelControl { Text = "Mevcut Pozisyon:", Dock = DockStyle.Fill }, 0, 1);
            lblMevcutPozisyon = new LabelControl
            {
                Text = "0.00 mm",
                Dock = DockStyle.Fill,
                Appearance = { Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.DarkBlue, TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center } }
            };
            panel.SetColumnSpan(lblMevcutPozisyon, 3);
            panel.Controls.Add(lblMevcutPozisyon, 1, 1);
            
            // Row 2
            var lblInfo = new LabelControl
            {
                Text = "?? Test pozisyonunu girin ve 'Pozisyona Git' butonuna basýn. Sonucu kontrol edin. Doðru ise kalibrasyon tamamdýr!",
                Dock = DockStyle.Fill,
                Appearance = { ForeColor = Color.DarkBlue },
                AutoSizeMode = LabelAutoSizeMode.Vertical
            };
            panel.SetColumnSpan(lblInfo, 4);
            panel.Controls.Add(lblInfo, 0, 2);
            
            grpPozisyonTest.Controls.Add(panel);
            
            // Real-time pozisyon güncellemesi için timer
            var timer = new System.Windows.Forms.Timer { Interval = 500 };
            timer.Tick += async (s, e) =>
            {
                if (!grpPozisyonTest.Enabled || IsDisposed)
                {
                    try { timer.Stop(); timer.Dispose(); } catch { }
                    return;
                }
                
                try
                {
                    var result = await ReadTagAsync("Mevcut_Pozisyon");
                    if (result.success && result.value != null)
                    {
                        if (!IsDisposed && lblMevcutPozisyon != null && !lblMevcutPozisyon.IsDisposed)
                        {
                            lblMevcutPozisyon.Text = $"{result.value:F2} mm";
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Sessiz baþarýsýz
                    _logger?.Debug(GetType().Name, $"Pozisyon okuma hatasý (timer): {ex.Message}");
                }
            };
            
            this.FormClosing += (s, e) => 
            {
                try { timer?.Stop(); timer?.Dispose(); } catch { }
            };
            
            timer.Start();
        }

        private void CreateLogGroup()
        {
            grpLog = new GroupControl
            {
                Text = "?? Ýþlem Geçmiþi",
                Width = 860,
                Height = 150
            };
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            
            txtLog = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { ReadOnly = true, ScrollBars = ScrollBars.Vertical }
            };
            panel.Controls.Add(txtLog);
            
            btnOncekiKalibrasyonlar = new SimpleButton
            {
                Text = "?? Önceki Kalibrasyonlar",
                Dock = DockStyle.Bottom,
                Height = 35,
                Appearance = { Font = new Font("Segoe UI", 9f) }
            };
            btnOncekiKalibrasyonlar.Click += BtnOncekiKalibrasyonlar_Click;
            panel.Controls.Add(btnOncekiKalibrasyonlar);
            
            grpLog.Controls.Add(panel);
        }

        private MotorCalcEdit CreateMotorCalcEdit(string title, bool enableValidation, int decimalPlaces)
        {
            return new MotorCalcEdit
            {
                PromptTitle = title,
                EnableValidation = enableValidation,
                MinValue = enableValidation ? _motorConfig.MinHiz : decimal.MinValue,
                MaxValue = enableValidation ? _motorConfig.MaxHiz : decimal.MaxValue,
                DecimalPlaces = decimalPlaces,
                Dock = DockStyle.Fill
            };
        }

        #endregion

        #region Event Handlers

        private async void BtnHomeGit_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("?? Home iþlemi baþlatýlýyor...");
                UpdateDurum(KalibrasyonDurum.HomeGidiyor);
                
                SetControlsEnabled(false);
                _cts = new CancellationTokenSource();
                
                // Motor hazýr mý kontrol et
                AddLog("Motor hazýr durumu kontrol ediliyor...");
                if (!await WaitForMotorReady(5000))
                {
                    throw new Exception("Motor hazýr deðil! Lütfen motor durumunu kontrol edin.");
                }
                
                AddLog("? Motor hazýr");
                
                // Home parametrelerini yaz
                AddLog($"Home parametreleri ayarlanýyor (Hýz: {txtHomeHiz.GetIntValue()}, Yavaþ: {txtHomeYavas.GetIntValue()})...");
                
                if (!await WriteTagAsync("Home_Hiz", txtHomeHiz.GetIntValue()))
                {
                    throw new Exception("Home hýz deðeri yazýlamadý!");
                }
                
                if (!await WriteTagAsync("Home_Yavas", txtHomeYavas.GetIntValue()))
                {
                    throw new Exception("Home yavaþ deðeri yazýlamadý!");
                }
                
                await Task.Delay(100);
                AddLog("? Home parametreleri ayarlandý");
                
                // Home komutu gönder
                AddLog("Home komutu PLC'ye gönderiliyor...");
                if (!await WriteTagAsync("Home_Git", true))
                {
                    throw new Exception("Home komutu gönderilemedi!");
                }
                
                AddLog("? Home komutu gönderildi, motor hareket ediyor...");
                AddLog("? Home switch bekleniyor (PLC feedback izleniyor)...");
                
                // Home switch'i veya home tamamlandý sinyalini bekle (PLC FEEDBACK)
                if (await WaitForHomeSwitch(_motorConfig.HomeTimeout))
                {
                    AddLog("? Home switch algýlandý!");
                    
                    // Kýsa bir bekleme (PLC hareket durdurma)
                    await Task.Delay(500);
                    
                    // Home komutunu sýfýrla
                    await WriteTagAsync("Home_Git", false);
                    
                    AddLog("? Home iþlemi tamamlandý");
                    UpdateDurum(KalibrasyonDurum.ManuelHareket);
                    grpManuel.Enabled = true;
                    
                    XtraMessageBox.Show(
                        "Home iþlemi baþarýyla tamamlandý.\n\nÞimdi manuel hareket ile motoru istenen pozisyona getirin.",
                        "Baþarýlý",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    throw new Exception("Home switch algýlanamadý veya zaman aþýmý! Lütfen sensör baðlantýsýný kontrol edin.");
                }
            }
            catch (OperationCanceledException)
            {
                AddLog("?? Home iþlemi kullanýcý tarafýndan iptal edildi");
                UpdateDurum(KalibrasyonDurum.Hata, "Ýþlem iptal edildi");
            }
            catch (Exception ex)
            {
                AddLog($"? HATA: {ex.Message}");
                UpdateDurum(KalibrasyonDurum.Hata, ex.Message);
                XtraMessageBox.Show(
                    $"Home iþlemi baþarýsýz!\n\n{ex.Message}\n\nLütfen:\n• PLC baðlantýsýný kontrol edin\n• Motor durumunu kontrol edin\n• Home switch'i kontrol edin", 
                    "Hata", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
            finally
            {
                // Komutu her durumda sýfýrla
                try
                {
                    await WriteTagAsync("Home_Git", false);
                }
                catch { }
                
                SetControlsEnabled(true);
            }
        }

        private async void BtnIleri_MouseDown()
        {
            try
            {
                await WriteTagAsync("Ileri_Hiz", txtIleriHiz.GetIntValue());
                await WriteTagAsync("Ileri_Rampa_Hiz", txtIleriRampaHiz.GetIntValue());
                await WriteTagAsync("Ileri_Rampa_Yavas", txtIleriRampaYavas.GetIntValue());
                await Task.Delay(50);
                await WriteTagAsync("Ileri", true);
                AddLog("?? Ýleri hareket baþladý");
            }
            catch (Exception ex)
            {
                AddLog($"? Ýleri hareket hatasý: {ex.Message}");
            }
        }

        private async void BtnGeri_MouseDown()
        {
            try
            {
                await WriteTagAsync("Geri_Hiz", txtGeriHiz.GetIntValue());
                await WriteTagAsync("Geri_Rampa_Hiz", txtGeriRampaHiz.GetIntValue());
                await WriteTagAsync("Geri_Rampa_Yavas", txtGeriRampaYavas.GetIntValue());
                await Task.Delay(50);
                await WriteTagAsync("Geri", true);
                AddLog("?? Geri hareket baþladý");
            }
            catch (Exception ex)
            {
                AddLog($"? Geri hareket hatasý: {ex.Message}");
            }
        }

        private async void BtnDur_Click(object sender, EventArgs e)
        {
            try
            {
                await WriteTagAsync("Ileri", false);
                await WriteTagAsync("Geri", false);
                AddLog("?? Hareket durduruldu");
                
                // Manuel hareket tamamlandý, kalibrasyon grubunu aç
                if (!grpKalibrasyon.Enabled)
                {
                    grpKalibrasyon.Enabled = true;
                    UpdateDurum(KalibrasyonDurum.OlculenPozisyonGirisi);
                    AddLog("?? Manuel hareket tamamlandý. Þimdi ölçülen pozisyon deðerini girin.");
                }
            }
            catch (Exception ex)
            {
                AddLog($"? Dur komutu hatasý: {ex.Message}");
            }
        }

        private async void BtnEksenKalibrasyon_Click(object sender, EventArgs e)
        {
            try
            {
                var olculen = txtOlculenPozisyon.GetFloatValue();
                
                if (Math.Abs(olculen) < 0.01f)
                {
                    XtraMessageBox.Show("Lütfen geçerli bir ölçülen pozisyon deðeri girin!", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                AddLog($"?? Kalibrasyon baþlatýlýyor... Ölçülen: {olculen:F2} mm");
                UpdateDurum(KalibrasyonDurum.KalibrasyonYapiliyor);
                
                SetControlsEnabled(false);
                
                // Ölçülen pozisyonu yaz
                await WriteTagAsync("Olculen_Pozisyon", olculen);
                await Task.Delay(100);
                
                // Kalibrasyon komutunu gönder
                await WriteTagAsync("Eksen_Kalibrasyon", true);
                AddLog("Kalibrasyon komutu gönderildi...");
                await Task.Delay(500);
                await WriteTagAsync("Eksen_Kalibrasyon", false);
                
                // Çýkýþ pozisyonunu oku
                await Task.Delay(1000);
                var cikisResult = await ReadTagAsync("Cikis_Pozisyonu");
                if (cikisResult.success && cikisResult.value != null)
                {
                    float cikis = Convert.ToSingle(cikisResult.value);
                    lblCikisPozisyonu.Text = $"{cikis:F2} mm";
                    AddLog($"? Kalibrasyon tamamlandý. Çýkýþ Pozisyonu: {cikis:F2} mm");
                }
                
                // Rampa grubunu aç
                grpRampa.Enabled = true;
                UpdateDurum(KalibrasyonDurum.RampaAyarlaniyor);
                
                XtraMessageBox.Show(
                    "Kalibrasyon baþarýyla tamamlandý!\nÞimdi rampa ayarlarýný yapýn.",
                    "Baþarýlý",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AddLog($"? Kalibrasyon hatasý: {ex.Message}");
                UpdateDurum(KalibrasyonDurum.Hata, ex.Message);
                XtraMessageBox.Show($"Kalibrasyon hatasý:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private async void BtnRampaSet_Click(object sender, EventArgs e)
        {
            try
            {
                var hizlanma = txtRampaHizlanma.GetFloatValue();
                var yavaslama = txtRampaYavaslama.GetFloatValue();
                
                AddLog($"? Rampa ayarlarý yapýlýyor... Hýzlanma: {hizlanma:F1}, Yavaþlama: {yavaslama:F1}");
                
                SetControlsEnabled(false);
                
                await WriteTagAsync("RampaSet_Hizlanma", hizlanma);
                await WriteTagAsync("RampaSet_Yavaslama", yavaslama);
                await Task.Delay(100);
                
                await WriteTagAsync("Rampa_Set", true);
                await Task.Delay(500);
                await WriteTagAsync("Rampa_Set", false);
                
                AddLog("? Rampa ayarlarý tamamlandý");
                
                // Pozisyon test grubunu aç
                grpPozisyonTest.Enabled = true;
                UpdateDurum(KalibrasyonDurum.PozisyonTest);
                
                XtraMessageBox.Show(
                    "Rampa ayarlarý baþarýyla tamamlandý!\nÞimdi pozisyon testi yapýn.",
                    "Baþarýlý",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AddLog($"? Rampa ayarý hatasý: {ex.Message}");
                XtraMessageBox.Show($"Rampa ayarý hatasý:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private async void BtnPozisyonaGit_Click(object sender, EventArgs e)
        {
            try
            {
                var hedefPoz = txtTestPozisyon.GetFloatValue();
                var hiz = txtTestHiz.GetIntValue();
                
                AddLog($"?? Test hareketi baþlatýlýyor... Hedef: {hedefPoz:F2} mm, Hýz: {hiz} mm/min");
                UpdateDurum(KalibrasyonDurum.PozisyonTest, "Test hareketi yapýlýyor...");
                
                SetControlsEnabled(false);
                _cts = new CancellationTokenSource();
                
                // Motor hazýr mý?
                if (!await WaitForMotorReady(5000))
                {
                    throw new Exception("Motor hazýr deðil!");
                }
                
                // Parametreleri yaz
                if (!await WriteTagAsync("Pozisyon", hedefPoz))
                {
                    throw new Exception("Hedef pozisyon yazýlamadý!");
                }
                
                if (!await WriteTagAsync("Pozisyon_Hiz", hiz))
                {
                    throw new Exception("Hareket hýzý yazýlamadý!");
                }
                
                await Task.Delay(100);
                
                // Pozisyon komutu gönder
                if (!await WriteTagAsync("Pozisyon_Git", true))
                {
                    throw new Exception("Pozisyon komutu gönderilemedi!");
                }
                
                AddLog("? Pozisyon komutu gönderildi, motor hareket ediyor...");
                AddLog("? Hareket tamamlanmasý bekleniyor (PLC feedback izleniyor)...");
                
                // Hareket bitiþini bekle (PLC FEEDBACK)
                if (await WaitForMotionComplete(_motorConfig.HareketTimeout))
                {
                    // Komutu sýfýrla
                    await WriteTagAsync("Pozisyon_Git", false);
                    
                    // Kýsa bekleme sonrasý mevcut pozisyonu oku
                    await Task.Delay(500);
                    
                    var mevcutResult = await ReadTagAsync("Mevcut_Pozisyon");
                    if (mevcutResult.success && mevcutResult.value != null)
                    {
                        float mevcut = Convert.ToSingle(mevcutResult.value);
                        float fark = Math.Abs(mevcut - hedefPoz);
                        
                        lblMevcutPozisyon.Text = $"{mevcut:F2} mm";
                        AddLog($"? Hareket tamamlandý!");
                        AddLog($"   Hedef: {hedefPoz:F2} mm");
                        AddLog($"   Mevcut: {mevcut:F2} mm");
                        AddLog($"   Fark: {fark:F2} mm");
                        
                        bool basarili = fark < 1.0f; // 1mm tolerans
                        
                        if (basarili)
                        {
                            UpdateDurum(KalibrasyonDurum.Tamamlandi, "? Kalibrasyon baþarýyla tamamlandý!");
                            AddLog("?? ?? ?? KALÝBRASYON BAÞARILI! ?? ?? ??");
                            
                            // Kalibrasyon kaydýný kaydet
                            await SaveKalibrasyonLog(true, hedefPoz);
                            
                            XtraMessageBox.Show(
                                $"? KALÝBRASYON BAÞARILI!\n\n" +
                                $"Hedef Pozisyon: {hedefPoz:F2} mm\n" +
                                $"Mevcut Pozisyon: {mevcut:F2} mm\n" +
                                $"Fark: {fark:F2} mm\n\n" +
                                $"Tolerans içinde (%1mm)",
                                "Kalibrasyon Baþarýlý",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            AddLog($"?? UYARI: Pozisyon farký tolerans dýþýnda! ({fark:F2} mm > 1.00 mm)");
                            
                            var result = XtraMessageBox.Show(
                                $"?? Pozisyon farký tolerans dýþýnda!\n\n" +
                                $"Hedef: {hedefPoz:F2} mm\n" +
                                $"Mevcut: {mevcut:F2} mm\n" +
                                $"Fark: {fark:F2} mm (>1mm)\n\n" +
                                $"Olasý nedenler:\n" +
                                $"• Kalibrasyon deðerleri yanlýþ\n" +
                                $"• Mekanik sorun\n" +
                                $"• Encoder sorunu\n\n" +
                                $"Kalibrasyonu tekrarlamak ister misiniz?",
                                "Pozisyon Hatasý",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);
                            
                            if (result == DialogResult.Yes)
                            {
                                await SaveKalibrasyonLog(false, hedefPoz);
                                ResetKalibrasyon();
                            }
                            else
                            {
                                await SaveKalibrasyonLog(false, hedefPoz);
                            }
                        }
                    }
                    else
                    {
                        AddLog("?? Mevcut pozisyon okunamadý!");
                        throw new Exception("Hareket tamamlandý ama pozisyon feedback alýnamadý!");
                    }
                }
                else
                {
                    throw new Exception("Pozisyon hareketi zaman aþýmýna uðradý veya hata oluþtu!");
                }
            }
            catch (OperationCanceledException)
            {
                AddLog("?? Test hareketi kullanýcý tarafýndan iptal edildi");
                UpdateDurum(KalibrasyonDurum.Hata, "Ýþlem iptal edildi");
            }
            catch (Exception ex)
            {
                AddLog($"? HATA: {ex.Message}");
                UpdateDurum(KalibrasyonDurum.Hata, ex.Message);
                XtraMessageBox.Show(
                    $"Test hareketi baþarýsýz!\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                // Komutu her durumda sýfýrla
                try
                {
                    await WriteTagAsync("Pozisyon_Git", false);
                }
                catch { }
                
                SetControlsEnabled(true);
            }
        }

        private async void BtnOncekiKalibrasyonlar_Click(object sender, EventArgs e)
        {
            try
            {
                if (_kalibrasyonService == null)
                {
                    XtraMessageBox.Show("Kalibrasyon servisi kullanýlamýyor.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                var logs = await _kalibrasyonService.GetKalibrasyonLogs(_motorConfig.MotorName);
                
                if (logs == null || logs.Count == 0)
                {
                    XtraMessageBox.Show("Henüz kayýtlý kalibrasyon bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                var msg = $"SON {logs.Count} KALÝBRASYON KAYDI:\n\n";
                foreach (var log in logs)
                {
                    msg += $"[{log.Timestamp:dd.MM.yyyy HH:mm}] ";
                    msg += log.Basarili ? "?" : "?";
                    msg += $" Ölçülen: {log.OlculenPozisyon:F2} mm, ";
                    msg += $"Test: {log.TestPozisyon:F2} mm";
                    if (!log.Basarili && !string.IsNullOrEmpty(log.HataMesaji))
                        msg += $" - {log.HataMesaji}";
                    msg += "\n";
                }
                
                XtraMessageBox.Show(msg, "Kalibrasyon Geçmiþi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger?.Error(GetType().Name, "Kalibrasyon geçmiþi okuma hatasý", ex);
                XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Helper Methods

        private async void LoadInitialValues()
        {
            try
            {
                // Default deðerler
                txtHomeHiz.SetValue(1000);
                txtHomeYavas.SetValue(500);
                
                txtIleriHiz.SetValue(1000);
                txtIleriRampaHiz.SetValue(500);
                txtIleriRampaYavas.SetValue(500);
                
                txtGeriHiz.SetValue(1000);
                txtGeriRampaHiz.SetValue(500);
                txtGeriRampaYavas.SetValue(500);
                
                txtRampaHizlanma.SetValue(500);
                txtRampaYavaslama.SetValue(500);
                
                txtTestPozisyon.SetValue(100);
                txtTestHiz.SetValue(1000);
                
                // Son baþarýlý kalibrasyonu yükle
                if (_kalibrasyonService != null)
                {
                    var lastSuccess = await _kalibrasyonService.GetLastSuccessfulKalibrasyon(_motorConfig.MotorName);
                    if (lastSuccess != null)
                    {
                        AddLog($"?? Son baþarýlý kalibrasyon: {lastSuccess.Timestamp:dd.MM.yyyy HH:mm}");
                        AddLog($"   Ölçülen: {lastSuccess.OlculenPozisyon:F2} mm, Test: {lastSuccess.TestPozisyon:F2} mm");
                        
                        txtRampaHizlanma.SetValue(lastSuccess.RampaHizlanma);
                        txtRampaYavaslama.SetValue(lastSuccess.RampaYavaslama);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(GetType().Name, "Initial deðer yükleme hatasý", ex);
            }
        }

        private void AddLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddLog(message)));
                return;
            }
            
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.Text += $"[{timestamp}] {message}\r\n";
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetControlsEnabled(enabled)));
                return;
            }
            
            grpHome.Enabled = enabled;
            grpManuel.Enabled = enabled && _durum >= KalibrasyonDurum.ManuelHareket;
            grpKalibrasyon.Enabled = enabled && _durum >= KalibrasyonDurum.OlculenPozisyonGirisi;
            grpRampa.Enabled = enabled && _durum >= KalibrasyonDurum.RampaAyarlaniyor;
            grpPozisyonTest.Enabled = enabled && _durum >= KalibrasyonDurum.PozisyonTest;
        }

        private void ResetKalibrasyon()
        {
            UpdateDurum(KalibrasyonDurum.Bekliyor);
            grpManuel.Enabled = false;
            grpKalibrasyon.Enabled = false;
            grpRampa.Enabled = false;
            grpPozisyonTest.Enabled = false;
            lblCikisPozisyonu.Text = "0.00 mm";
            lblMevcutPozisyon.Text = "0.00 mm";
            AddLog("?? Kalibrasyon sýfýrlandý, baþtan baþlayýn.");
        }

        private async Task SaveKalibrasyonLog(bool basarili, float? testPozisyon = null)
        {
            try
            {
                if (_kalibrasyonService == null)
                    return;
                
                var log = new KalibrasyonLog
                {
                    MotorName = _motorConfig.MotorName,
                    Timestamp = DateTime.Now,
                    KullaniciAdi = Environment.UserName,
                    OlculenPozisyon = txtOlculenPozisyon.GetFloatValue(),
                    CikisPozisyonu = float.Parse(lblCikisPozisyonu.Text.Replace(" mm", "")),
                    RampaHizlanma = txtRampaHizlanma.GetFloatValue(),
                    RampaYavaslama = txtRampaYavaslama.GetFloatValue(),
                    TestPozisyon = testPozisyon,
                    TestBasarili = basarili,
                    Basarili = basarili,
                    HataMesaji = basarili ? null : "Pozisyon tolerans dýþýnda",
                    Notlar = $"Home Hýz: {txtHomeHiz.GetIntValue()}, Test Hýz: {txtTestHiz.GetIntValue()}"
                };
                
                await _kalibrasyonService.SaveKalibrasyonLog(log);
                AddLog("?? Kalibrasyon kaydý veritabanýna kaydedildi");
            }
            catch (Exception ex)
            {
                _logger?.Error(GetType().Name, "Kalibrasyon log kaydetme hatasý", ex);
            }
        }

        #endregion
    }
}
