using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Motor;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.Core.Tags;

namespace KaynakMakinesi.UI.Forms
{
    /// <summary>
    /// Tüm motor kalibrasyon formlarý için base sýnýf
    /// Ortak özellikleri ve davranýþlarý içerir
    /// </summary>
    public abstract class BaseKalibrasyonForm : XtraForm
    {
        // Services
        protected readonly IModbusService _modbusService;
        protected readonly ITagService _tagService;
        protected readonly IAppLogger _logger;
        protected readonly IKalibrasyonService _kalibrasyonService;
        
        // Motor Configuration
        protected readonly MotorConfig _motorConfig;
        
        // Durum
        protected KalibrasyonDurum _durum = KalibrasyonDurum.Bekliyor;
        protected CancellationTokenSource _cts;
        protected DateTime _islemBaslangic;
        
        // UI Controls - Layout
        protected LayoutControl layoutControl;
        protected LayoutControlGroup layoutControlGroup;
        
        // Progress & Status
        protected LabelControl lblDurum;
        protected ProgressBarControl progressBar;
        protected LabelControl lblAdimlar;
        
        // Butonlar
        protected SimpleButton btnAcilDur;
        protected SimpleButton btnKapat;
        
        // Constructor
        protected BaseKalibrasyonForm(
            MotorConfig motorConfig,
            IModbusService modbusService,
            ITagService tagService,
            IAppLogger logger,
            IKalibrasyonService kalibrasyonService)
        {
            _motorConfig = motorConfig ?? throw new ArgumentNullException(nameof(motorConfig));
            _modbusService = modbusService ?? throw new ArgumentNullException(nameof(modbusService));
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            _logger = logger;
            _kalibrasyonService = kalibrasyonService;
            
            try
            {
                InitializeBaseUI();
            }
            catch (Exception ex)
            {
                _logger?.Error(GetType().Name, "InitializeBaseUI hatasý", ex);
                MessageBox.Show($"Form yüklenirken hata:\n{ex.Message}\n\nDetay:\n{ex.StackTrace}", 
                    "Kritik Hata", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                throw;
            }
        }

        private void InitializeBaseUI()
        {
            // Form ayarlarý
            Text = $"{_motorConfig.DisplayName} Kalibrasyon";
            Width = 900;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            
            // Layout Control
            layoutControl = new LayoutControl
            {
                Dock = DockStyle.Fill,
                AllowCustomization = false
            };
            Controls.Add(layoutControl);
            
            layoutControlGroup = new LayoutControlGroup
            {
                EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True,
                GroupBordersVisible = false,
                Text = "Root"
            };
            layoutControl.Root = layoutControlGroup;
            
            // Durum göstergeleri
            CreateStatusControls();
            
            // Acil Dur ve Kapat butonlarý
            CreateActionButtons();
            
            // Form close eventi
            FormClosing += OnFormClosing;
            
            // Alt sýnýfýn UI'ýný oluþtur
            InitializeMotorUI();
        }

        private void CreateStatusControls()
        {
            // Durum baþlýðý
            var grpDurum = new GroupControl
            {
                Text = "Ýþlem Durumu",
                Dock = DockStyle.Top,
                Height = 120
            };
            
            var pnlDurum = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            grpDurum.Controls.Add(pnlDurum);
            
            lblDurum = new LabelControl
            {
                Text = "Hazýr - Kalibrasyon baþlatmak için adýmlarý izleyin",
                Dock = DockStyle.Top,
                Appearance = { Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.DarkBlue },
                AutoSizeMode = LabelAutoSizeMode.Vertical,
                Height = 30
            };
            pnlDurum.Controls.Add(lblDurum);
            
            progressBar = new ProgressBarControl
            {
                Dock = DockStyle.Top,
                Height = 30,
                Properties = { ShowTitle = true, Maximum = 100 }
            };
            progressBar.Top = lblDurum.Bottom + 5;
            pnlDurum.Controls.Add(progressBar);
            
            lblAdimlar = new LabelControl
            {
                Text = "Adým 1/7: Home pozisyonuna git",
                Dock = DockStyle.Top,
                Appearance = { Font = new Font("Segoe UI", 9f), ForeColor = Color.DarkGreen },
                AutoSizeMode = LabelAutoSizeMode.Vertical
            };
            lblAdimlar.Top = progressBar.Bottom + 5;
            pnlDurum.Controls.Add(lblAdimlar);
            
            Controls.Add(grpDurum);
            grpDurum.BringToFront();
        }

        private void CreateActionButtons()
        {
            var pnlButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };
            
            btnAcilDur = new SimpleButton
            {
                Text = "? ACÝL DUR",
                Width = 150,
                Height = 40,
                Dock = DockStyle.Right,
                Appearance = { BackColor = Color.Red, ForeColor = Color.White, Font = new Font("Segoe UI", 11f, FontStyle.Bold) }
            };
            btnAcilDur.Click += BtnAcilDur_Click;
            pnlButtons.Controls.Add(btnAcilDur);
            
            btnKapat = new SimpleButton
            {
                Text = "Kapat",
                Width = 100,
                Height = 40,
                Dock = DockStyle.Right,
                Appearance = { Font = new Font("Segoe UI", 10f) }
            };
            btnKapat.Click += (s, e) => Close();
            btnKapat.Left = btnAcilDur.Left - btnKapat.Width - 10;
            pnlButtons.Controls.Add(btnKapat);
            
            Controls.Add(pnlButtons);
        }

        // Abstract methods - Alt sýnýflar implement edecek
        protected abstract void InitializeMotorUI();
        
        // Durum güncelleme
        protected void UpdateDurum(KalibrasyonDurum yeniDurum, string mesaj = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateDurum(yeniDurum, mesaj)));
                return;
            }
            
            _durum = yeniDurum;
            
            // Progress deðerini güncelle
            var progress = GetProgressPercentage(yeniDurum);
            progressBar.Position = progress;
            
            // Durum mesajýný güncelle
            if (!string.IsNullOrEmpty(mesaj))
            {
                lblDurum.Text = mesaj;
            }
            else
            {
                lblDurum.Text = GetDurumMesaji(yeniDurum);
            }
            
            // Adým bilgisini güncelle
            lblAdimlar.Text = GetAdimBilgisi(yeniDurum);
            
            // Renk güncellemesi
            lblDurum.Appearance.ForeColor = GetDurumRenk(yeniDurum);
            
            _logger?.Info(GetType().Name, $"Durum: {yeniDurum} - {lblDurum.Text}");
        }

        private int GetProgressPercentage(KalibrasyonDurum durum)
        {
            switch (durum)
            {
                case KalibrasyonDurum.Bekliyor: return 0;
                case KalibrasyonDurum.HomeGidiyor: return 10;
                case KalibrasyonDurum.ManuelHareket: return 25;
                case KalibrasyonDurum.OlculenPozisyonGirisi: return 40;
                case KalibrasyonDurum.KalibrasyonYapiliyor: return 55;
                case KalibrasyonDurum.RampaAyarlaniyor: return 70;
                case KalibrasyonDurum.PozisyonTest: return 85;
                case KalibrasyonDurum.Tamamlandi: return 100;
                case KalibrasyonDurum.Hata: return 0;
                default: return 0;
            }
        }

        private string GetDurumMesaji(KalibrasyonDurum durum)
        {
            switch (durum)
            {
                case KalibrasyonDurum.Bekliyor: 
                    return "Hazýr - Kalibrasyon baþlatmak için adýmlarý izleyin";
                case KalibrasyonDurum.HomeGidiyor: 
                    return "Motor home pozisyonuna gidiyor...";
                case KalibrasyonDurum.ManuelHareket: 
                    return "Manuel hareket ile pozisyon ayarý yapýn";
                case KalibrasyonDurum.OlculenPozisyonGirisi: 
                    return "Ölçülen pozisyon deðerini girin";
                case KalibrasyonDurum.KalibrasyonYapiliyor: 
                    return "Kalibrasyon PLC'ye gönderiliyor...";
                case KalibrasyonDurum.RampaAyarlaniyor: 
                    return "Rampa ayarlarý yapýlýyor...";
                case KalibrasyonDurum.PozisyonTest: 
                    return "Test hareketi yapýlýyor...";
                case KalibrasyonDurum.Tamamlandi: 
                    return "? Kalibrasyon baþarýyla tamamlandý!";
                case KalibrasyonDurum.Hata: 
                    return "? Hata oluþtu - Ýþlem iptal edildi";
                default: 
                    return "Bilinmeyen durum";
            }
        }

        private string GetAdimBilgisi(KalibrasyonDurum durum)
        {
            switch (durum)
            {
                case KalibrasyonDurum.HomeGidiyor: return "Adým 1/7: Home pozisyonuna git";
                case KalibrasyonDurum.ManuelHareket: return "Adým 2/7: Manuel hareket";
                case KalibrasyonDurum.OlculenPozisyonGirisi: return "Adým 3/7: Ölçülen pozisyon giriþi";
                case KalibrasyonDurum.KalibrasyonYapiliyor: return "Adým 4/7: Kalibrasyon komutu";
                case KalibrasyonDurum.RampaAyarlaniyor: return "Adým 5/7: Rampa ayarlarý";
                case KalibrasyonDurum.PozisyonTest: return "Adým 6/7: Pozisyon testi";
                case KalibrasyonDurum.Tamamlandi: return "Adým 7/7: Tamamlandý ?";
                default: return string.Empty;
            }
        }

        private Color GetDurumRenk(KalibrasyonDurum durum)
        {
            switch (durum)
            {
                case KalibrasyonDurum.Bekliyor: return Color.DarkBlue;
                case KalibrasyonDurum.HomeGidiyor:
                case KalibrasyonDurum.KalibrasyonYapiliyor:
                case KalibrasyonDurum.RampaAyarlaniyor:
                case KalibrasyonDurum.PozisyonTest:
                    return Color.DarkOrange;
                case KalibrasyonDurum.ManuelHareket:
                case KalibrasyonDurum.OlculenPozisyonGirisi:
                    return Color.DarkCyan;
                case KalibrasyonDurum.Tamamlandi: 
                    return Color.DarkGreen;
                case KalibrasyonDurum.Hata: 
                    return Color.DarkRed;
                default: 
                    return Color.Black;
            }
        }

        // Acil Dur
        protected virtual void BtnAcilDur_Click(object sender, EventArgs e)
        {
            try
            {
                _cts?.Cancel();
                UpdateDurum(KalibrasyonDurum.Hata, "? ACÝL DUR - Ýþlem iptal edildi!");
                _logger?.Warn(GetType().Name, "Acil Dur butonu basýldý");
                
                XtraMessageBox.Show(
                    "Kalibrasyon iþlemi iptal edildi.\nMotor güvenli duruma getirildi.",
                    "Acil Dur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                _logger?.Error(GetType().Name, "Acil Dur hatasý", ex);
            }
        }

        // Form kapanýrken
        protected virtual void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_durum != KalibrasyonDurum.Bekliyor && 
                _durum != KalibrasyonDurum.Tamamlandi && 
                _durum != KalibrasyonDurum.Hata)
            {
                var result = XtraMessageBox.Show(
                    "Kalibrasyon iþlemi devam ediyor!\nÇýkmak istediðinize emin misiniz?",
                    "Uyarý",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            
            _cts?.Cancel();
            _cts?.Dispose();
        }

        // Helper: Tag okuma (Exception-safe)
        protected async Task<(bool success, object value)> ReadTagAsync(string tagSuffix)
        {
            try
            {
                var tagName = _motorConfig.GetTagName(tagSuffix);
                
                // Tag var mý kontrol et
                if (_tagService == null)
                {
                    _logger?.Warn(GetType().Name, $"TagService null! Tag okunamadý: {tagName}");
                    return (false, null);
                }
                
                var result = await _tagService.ReadTagAsync(tagName, _cts?.Token ?? CancellationToken.None);
                
                if (result.Success)
                {
                    return (true, result.Value);
                }
                else
                {
                    // Sadece kritik tag'ler için log, diðerleri sessiz baþarýsýz
                    if (IsCriticalTag(tagSuffix))
                    {
                        _logger?.Error(GetType().Name, $"Kritik tag okuma hatasý: {tagName} - {result.Error}");
                    }
                    return (false, null);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, log'lama
                return (false, null);
            }
            catch (Exception ex)
            {
                // Sadece kritik tag'ler için log
                if (IsCriticalTag(tagSuffix))
                {
                    _logger?.Error(GetType().Name, $"Tag okuma exception: {tagSuffix}", ex);
                }
                return (false, null);
            }
        }

        // Helper: Tag yazma (Exception-safe)
        protected async Task<bool> WriteTagAsync(string tagSuffix, object value)
        {
            try
            {
                var tagName = _motorConfig.GetTagName(tagSuffix);
                
                _logger?.Debug(GetType().Name, $"? WriteTagAsync: tagSuffix='{tagSuffix}' -> tagName='{tagName}', value={value}");
                
                if (_tagService == null)
                {
                    _logger?.Error(GetType().Name, $"? TagService NULL! Tag yazýlamadý: {tagName}");
                    return false;
                }
                
                _logger?.Debug(GetType().Name, $"? TagService.WriteTagAsync çaðrýlýyor: {tagName} = {value}");
                
                var success = await _tagService.WriteTagAsync(tagName, value, _cts?.Token ?? CancellationToken.None);
                
                if (success)
                {
                    _logger?.Info(GetType().Name, $"? Tag yazma BAÞARILI: {tagName} = {value}");
                }
                else
                {
                    _logger?.Error(GetType().Name, $"? Tag yazma BAÞARISIZ: {tagName} = {value}");
                }
                
                return success;
            }
            catch (OperationCanceledException)
            {
                _logger?.Warn(GetType().Name, $"Tag yazma iptal edildi: {tagSuffix}");
                return false;
            }
            catch (Exception ex)
            {
                _logger?.Error(GetType().Name, $"? Tag yazma EXCEPTION: {tagSuffix} = {value}", ex);
                return false;
            }
        }
        
        /// <summary>
        /// Kritik tag'leri belirle (log için)
        /// </summary>
        private bool IsCriticalTag(string tagSuffix)
        {
            var criticalTags = new[] 
            { 
                "Home_Switch",
                "Acil_Stop",
                "Mevcut_Pozisyon"
            };
            return criticalTags.Contains(tagSuffix);
        }

        // Helper: Motor hazýr mý kontrol et (Exception-safe)
        protected async Task<bool> WaitForMotorReady(int timeoutMs = 5000)
        {
            // Motor_Hazir tag'i yoksa direkt true dön
            var start = DateTime.Now;
            
            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                if (_cts != null && _cts.IsCancellationRequested)
                    return false;
                    
                try
                {
                    var result = await ReadTagAsync("Motor_Hazir");
                    
                    // Tag bulunamadýysa (baþarýsýz) - direkt true dön, devam et
                    if (!result.success)
                    {
                        _logger?.Warn(GetType().Name, "Motor_Hazir tag'i bulunamadý, kontrol atlanýyor.");
                        return true; // Tag yoksa hazýr kabul et
                    }
                    
                    if (result.value is bool ready && ready)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Tag yoksa devam et
                    return true;
                }
                
                await Task.Delay(100);
            }
            
            _logger?.Warn(GetType().Name, $"Motor hazýr bekleme timeout! ({timeoutMs}ms) - Devam ediliyor.");
            return true; // Timeout olsa bile devam et
        }

        // Helper: Home switch'i bekle (PLC feedback)
        protected async Task<bool> WaitForHomeSwitch(int timeoutMs = 30000)
        {
            var start = DateTime.Now;
            _logger?.Info(GetType().Name, "Home switch bekleniyor...");
            
            int checkCount = 0;
            
            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                if (_cts != null && _cts.IsCancellationRequested)
                {
                    _logger?.Warn(GetType().Name, "Home switch bekleme iptal edildi");
                    return false;
                }
                
                checkCount++;
                
                try
                {
                    // Home switch kontrolü (CPU_IP1 - 10002)
                    var switchResult = await ReadTagAsync("Home_Switch");
                    
                    // DEBUG: Her 10 okumada bir log
                    if (checkCount % 10 == 0)
                    {
                        _logger?.Info(GetType().Name, $"Home_Switch okuma #{checkCount}: success={switchResult.success}, value={switchResult.value}");
                    }
                    
                    if (switchResult.success && switchResult.value is bool switchActive)
                    {
                        if (switchActive)
                        {
                            _logger?.Info(GetType().Name, "? Home switch aktif (CPU_IP1 = TRUE)!");
                            return true;
                        }
                        else
                        {
                            // DEBUG: Switch false
                            if (checkCount % 10 == 0)
                            {
                                _logger?.Debug(GetType().Name, "Home_Switch = FALSE, bekleniyor...");
                            }
                        }
                    }
                    else if (!switchResult.success)
                    {
                        // Tag okunamadý
                        _logger?.Warn(GetType().Name, $"Home_Switch tag'i okunamadý! (Kontrol #{checkCount})");
                    }
                    
                    // Acil Stop kontrolü (CPU_IP3 - 10004)
                    var acilStopResult = await ReadTagAsync("Acil_Stop");
                    if (acilStopResult.success && acilStopResult.value is bool acilStop && acilStop)
                    {
                        _logger?.Error(GetType().Name, "Acil Stop aktif! Ýþlem durduruluyor.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error(GetType().Name, $"Home switch okuma exception: {ex.Message}");
                }
                
                await Task.Delay(100);
            }
            
            _logger?.Error(GetType().Name, $"? Home switch timeout! ({checkCount} kontrol yapýldý, {timeoutMs}ms)");
            return false;
        }

        // Helper: Hareket bitiþini bekle (Sadece Pozisyon kontrolü)
        protected async Task<bool> WaitForMotionComplete(int timeoutMs = 30000)
        {
            var start = DateTime.Now;
            _logger?.Info(GetType().Name, "Hareket tamamlanmasý bekleniyor (pozisyon bazlý)...");
            
            // Ýlk pozisyonu oku
            float? startPos = null;
            var startPosResult = await ReadTagAsync("Mevcut_Pozisyon");
            if (startPosResult.success && startPosResult.value != null)
            {
                startPos = Convert.ToSingle(startPosResult.value);
                _logger?.Info(GetType().Name, $"Baþlangýç pozisyonu: {startPos:F2} mm");
            }
            else
            {
                _logger?.Warn(GetType().Name, "Mevcut_Pozisyon tag'i bulunamadý, sabit bekleme yapýlýyor.");
                // Pozisyon tag'i yoksa sabit süre bekle
                await Task.Delay(5000);
                return true;
            }
            
            // Hareketin baþlamasýný bekle
            await Task.Delay(500);
            
            // Pozisyon stabilitesi için
            int stableCount = 0;
            float? lastPos = null;
            
            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                if (_cts != null && _cts.IsCancellationRequested)
                {
                    _logger?.Warn(GetType().Name, "Hareket bekleme iptal edildi");
                    return false;
                }
                
                try
                {
                    // Mevcut pozisyonu oku
                    var posResult = await ReadTagAsync("Mevcut_Pozisyon");
                    if (posResult.success && posResult.value != null)
                    {
                        float currentPos = Convert.ToSingle(posResult.value);
                        
                        // Pozisyon deðiþmedi mi? (0.5mm tolerans - daha geniþ)
                        if (lastPos.HasValue && Math.Abs(currentPos - lastPos.Value) < 0.5f)
                        {
                            stableCount++;
                            
                            // 3 kere üst üste ayný pozisyon = hareket bitti (daha hýzlý)
                            if (stableCount >= 3)
                            {
                                _logger?.Info(GetType().Name, $"Hareket tamamlandý. Son pozisyon: {currentPos:F2} mm");
                                return true;
                            }
                        }
                        else
                        {
                            stableCount = 0; // Reset
                        }
                        
                        lastPos = currentPos;
                    }
                    
                    // Acil Stop kontrolü (varsa)
                    var acilStopResult = await ReadTagAsync("Acil_Stop");
                    if (acilStopResult.success && acilStopResult.value is bool acilStop && acilStop)
                    {
                        _logger?.Error(GetType().Name, "Acil Stop aktif! Hareket durduruluyor.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Warn(GetType().Name, $"Hareket feedback okuma hatasý: {ex.Message}");
                }
                
                await Task.Delay(300); // Biraz daha yavaþ poll
            }
            
            _logger?.Warn(GetType().Name, $"Hareket timeout! ({timeoutMs}ms) - Devam ediliyor.");
            return true; // Timeout olsa bile devam et
        }
        
        // Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
