using DevExpress.XtraEditors;
using KaynakMakinesi.Core.Abstractions;
using KaynakMakinesi.Core.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaynakMakinesi.UI
{
    public partial class SettingsForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly ISettingsStore<AppSettings> _store;
        private AppSettings _settings;
        public SettingsForm(ISettingsStore<AppSettings> store)
        {
            InitializeComponent();
            _store = store;

            this.Load += SettingsForm_Load;

            btnSave.Click += btnSave_Click;
            btnApply.Click += btnApply_Click;
            btnCancel.Click += btnCancel_Click;
            btnReload.Click += btnReload_Click;
            btnDefaults.Click += btnDefaults_Click;

            btnTestConnection.Click += btnTestConnection_Click;
            txtDbFileName.ButtonClick += txtDbFileName_ButtonClick;
        }
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            // Combo items
            cmbLogMinLevel.Properties.Items.Clear();
            cmbLogMinLevel.Properties.Items.AddRange(new[] { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" });

            // Spin limitler
            spnPlcPort.Properties.MinValue = 1; spnPlcPort.Properties.MaxValue = 65535;
            spnPlcUnitId.Properties.MinValue = 1; spnPlcUnitId.Properties.MaxValue = 247;
            spnPlcTimeoutMs.Properties.MinValue = 100; spnPlcTimeoutMs.Properties.MaxValue = 30000;
            spnHeartbeatAddress.Properties.MinValue = 0; spnHeartbeatAddress.Properties.MaxValue = 65535;
            spnHeartbeatIntervalMs.Properties.MinValue = 100; spnHeartbeatIntervalMs.Properties.MaxValue = 10000;
            spnLogKeepInMemory.Properties.MinValue = 100; spnLogKeepInMemory.Properties.MaxValue = 20000;

            LoadSettingsToUi();
        }

        private void LoadSettingsToUi()
        {
            _settings = _store.Load();

            txtPlcIp.Text = _settings.Plc.Ip;
            spnPlcPort.Value = _settings.Plc.Port;
            spnPlcUnitId.Value = _settings.Plc.UnitId;
            spnPlcTimeoutMs.Value = _settings.Plc.TimeoutMs;
            spnHeartbeatAddress.Value = _settings.Plc.HeartbeatAddress;
            spnHeartbeatIntervalMs.Value = _settings.Plc.HeartbeatIntervalMs;

            txtDbFileName.Text = _settings.Database.FileName;
            txtDbFullPath.Text = GetDbFullPath(_settings.Database.FileName);

            cmbLogMinLevel.EditValue = _settings.Logging.MinLevel;
            spnLogKeepInMemory.Value = _settings.Logging.KeepInMemory;

            lblTestResult.Text = "-";
            dxErrorProvider1.Clear();
        }

        private bool ValidateInputs()
        {
            dxErrorProvider1.Clear();

            if (!IPAddress.TryParse(txtPlcIp.Text.Trim(), out _))
            {
                dxErrorProvider1.SetError(txtPlcIp, "Geçerli IP gir.");
                tabSettings.SelectedTabPage = tabPlc;
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDbFileName.Text))
            {
                dxErrorProvider1.SetError(txtDbFileName, "DB dosya adı boş olamaz.");
                tabSettings.SelectedTabPage = tabDatabase;
                return false;
            }

            if (cmbLogMinLevel.EditValue == null || string.IsNullOrWhiteSpace(cmbLogMinLevel.EditValue.ToString()))
            {
                dxErrorProvider1.SetError(cmbLogMinLevel, "Min log seviyesini seç.");
                tabSettings.SelectedTabPage = tabLogging;
                return false;
            }

            return true;
        }

        private void ApplyFromUiToSettings()
        {
            _settings.Plc.Ip = txtPlcIp.Text.Trim();
            _settings.Plc.Port = (int)spnPlcPort.Value;
            _settings.Plc.UnitId = (byte)spnPlcUnitId.Value;
            _settings.Plc.TimeoutMs = (int)spnPlcTimeoutMs.Value;
            _settings.Plc.HeartbeatAddress = (ushort)spnHeartbeatAddress.Value;
            _settings.Plc.HeartbeatIntervalMs = (int)spnHeartbeatIntervalMs.Value;

            _settings.Database.FileName = txtDbFileName.Text.Trim();

            _settings.Logging.MinLevel = cmbLogMinLevel.EditValue.ToString();
            _settings.Logging.KeepInMemory = (int)spnLogKeepInMemory.Value;
        }

        private void SaveSettings()
        {
            ApplyFromUiToSettings();
            _store.Save(_settings); // SettingsChanged -> supervisor reconnect
            txtDbFullPath.Text = GetDbFullPath(_settings.Database.FileName);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;
            SaveSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;
            SaveSettings();
            XtraMessageBox.Show("Ayarlar uygulandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            LoadSettingsToUi();
        }

        private void btnDefaults_Click(object sender, EventArgs e)
        {
            // İstersen sabit defaultları bas:
            txtPlcIp.Text = "192.168.0.10";
            spnPlcPort.Value = 502;
            spnPlcUnitId.Value = 1;
            spnPlcTimeoutMs.Value = 1500;
            spnHeartbeatAddress.Value = 0;
            spnHeartbeatIntervalMs.Value = 750;

            txtDbFileName.Text = "app.db";
            cmbLogMinLevel.EditValue = "Info";
            spnLogKeepInMemory.Value = 2000;
        }

        private async void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            btnTestConnection.Enabled = false;
            lblTestResult.Text = "Test ediliyor...";

            try
            {
                var ip = txtPlcIp.Text.Trim();
                var port = (int)spnPlcPort.Value;
                var timeout = (int)spnPlcTimeoutMs.Value;

                using (var tcp = new TcpClient())
                {
                    var connectTask = tcp.ConnectAsync(ip, port);
                    var done = await Task.WhenAny(connectTask, Task.Delay(timeout));
                    if (done != connectTask || !tcp.Connected)
                        throw new TimeoutException("Zaman aşımı / port kapalı.");

                    lblTestResult.Text = "OK (TCP)";
                }
            }
            catch (Exception ex)
            {
                lblTestResult.Text = "FAIL: " + ex.Message;
            }
            finally
            {
                btnTestConnection.Enabled = true;
            }
        }

        private void txtDbFileName_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "SQLite DB (*.db)|*.db|All files (*.*)|*.*";
                dlg.FileName = txtDbFileName.Text;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // Sadece dosya adını tutmak istiyorsan:
                    txtDbFileName.Text = System.IO.Path.GetFileName(dlg.FileName);
                    txtDbFullPath.Text = GetDbFullPath(txtDbFileName.Text);
                }
            }
        }

        private string GetDbFullPath(string fileName)
        {
            var appFolder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KaynakMakinesi");

            return System.IO.Path.Combine(appFolder, fileName);
        }
    }
}