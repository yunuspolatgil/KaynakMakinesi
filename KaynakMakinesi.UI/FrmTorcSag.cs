using DevExpress.XtraEditors;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.Core.Tags;
using KaynakMakinesi.UI.Utils;
using System;
using System.Threading;
using System.Windows.Forms;

namespace KaynakMakinesi.UI
{
    public partial class FrmTorcSag : XtraForm
    {
        private readonly IModbusService _modbusService;
        private readonly IAppLogger _log;
        private readonly ITagService _tagService;

        public FrmTorcSag(IModbusService modbusService, IAppLogger log, ITagService tagService = null)
        {
            InitializeComponent();
            _modbusService = modbusService ?? throw new ArgumentNullException(nameof(modbusService));
            _log = log;
            _tagService = tagService;
        }

        private void buttonEdit1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (UiHelpers.TryTouchNumeric(this, "Rampa Hızlanma", 0m, out var val))
                txtRampaHizlanma.Text = val.ToString();
        }

        private void buttonEdit2_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (UiHelpers.TryTouchNumeric(this, "Rampa Yavaşlama", 0m, out var val))
                txtRampaYavaslama.Text = val.ToString();
        }

        private async void btnEksenPozisyonKalibrasyon_Click(object sender, EventArgs e)
        {
            try
            {
                if (!decimal.TryParse(txtOlculenPozisyon.Text, out var value))
                {
                    XtraMessageBox.Show("Geçerli bir sayı girin", "Hata");
                    return;
                }

                var ok = await _modbusService.WriteTextAsync("42029", value.ToString(), CancellationToken.None);
                XtraMessageBox.Show(ok ? "Yazma OK" : "Yazma başarısız", "Bilgi");
            }
            catch (Exception ex)
            {
                _log?.Error(nameof(FrmTorcSag), "Pozisyon yazma hatası", ex);
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata");
            }
        }

        private async void btnRampaHizlanmaYaz_Click(object sender, EventArgs e)
        {
            try
            {
                if (!decimal.TryParse(txtRampaHizlanma.Text, out var value))
                {
                    XtraMessageBox.Show("Geçerli bir sayı girin", "Hata");
                    return;
                }

                var ok = await _modbusService.WriteTextAsync("MW0", value.ToString(), CancellationToken.None);
                XtraMessageBox.Show(ok ? "Yazma OK" : "Yazma başarısız", "Bilgi");
            }
            catch (Exception ex)
            {
                _log?.Error(nameof(FrmTorcSag), "Rampa yazma hatası", ex);
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata");
            }
        }

        private async void FrmTorcSag_Load(object sender, EventArgs e)
        {
            await LoadTagValues();
        }

        private async System.Threading.Tasks.Task LoadTagValues()
        {
            try
            {
                var result1 = await _modbusService.ReadAutoAsync("42029", CancellationToken.None);
                if (result1.Success)
                    txtOlculenPozisyon.Text = result1.Value?.ToString();

                var result2 = await _modbusService.ReadAutoAsync("MW0", CancellationToken.None);
                if (result2.Success)
                    txtRampaHizlanma.Text = result2.Value?.ToString();

                var result3 = await _modbusService.ReadAutoAsync("MW1", CancellationToken.None);
                if (result3.Success)
                    txtRampaYavaslama.Text = result3.Value?.ToString();
            }
            catch (Exception ex)
            {
                _log?.Error(nameof(FrmTorcSag), "Tag okuma hatası", ex);
            }
        }

        private async void btnTumDegerleriOku_Click(object sender, EventArgs e)
        {
            await LoadTagValues();
            XtraMessageBox.Show("Tüm değerler yenilendi.", "Bilgi");
        }
    }
}