using DevExpress.XtraEditors;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.UI.Utils;
using System;
using System.Threading;

namespace KaynakMakinesi.UI
{
    public partial class FrmTorcSag : XtraForm
    {
        private readonly IModbusService _modbusService;
        private readonly IAppLogger _log;

        public FrmTorcSag(IModbusService modbusService, IAppLogger log)
        {
            InitializeComponent();
            _modbusService = modbusService ?? throw new ArgumentNullException(nameof(modbusService));
            _log = log;
        }

        private void textEdit1_EditValueChanged(object sender, EventArgs e)
        {
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
            var text = (txtOlculenPozisyon.Text ?? "").Trim();
            if (!int.TryParse(text, out var i))
            {
                XtraMessageBox.Show("Geçerli bir tam sayı girin.", "Uyarı");
                return;
            }

            var ok = await _modbusService
                .WriteTextAsync("Olculen_Pozisyon", i.ToString(), CancellationToken.None)
                .ConfigureAwait(false);
            // TagManager’da RampaPozKalibrasyonInt tag’i: Address=42029, Type=Int32

            BeginInvoke((Action)(() =>
            {
                XtraMessageBox.Show(ok ? "Yazma OK" : "Yazma başarısız", "Bilgi");
                if (ok) txtOlculenPozisyon.Text = i.ToString();
            }));
        }

        private async void btnRampaHizlanmaYaz_Click(object sender, EventArgs e)
        {
            var text = (txtRampaHizlanma.Text ?? "").Trim();

            if (!int.TryParse(text, out var i))
            {
                XtraMessageBox.Show("Geçerli bir tam sayı girin.", "Uyarı");
                return;
            }

            var ok = await _modbusService
                .WriteTextAsync("RampaHizlanma", i.ToString(), CancellationToken.None)
                .ConfigureAwait(false);

            BeginInvoke((Action)(() =>
            {
                XtraMessageBox.Show(ok ? "Yazma OK" : "Yazma başarısız", "Bilgi");
            }));
        }

        private void FrmTorcSag_Load(object sender, EventArgs e)
        {
            // Tag tanımı (RampaPozKalibrasyon) app.db'de zaten olmalı.
            // Gerekirse bu iş, Tag Manager veya başlangıçta merkezi bir yerde yapılmalıdır.
        }
    }
}