using KaynakMakinesi.Core.Abstractions;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.Core.Settings;
using KaynakMakinesi.Infrastructure.Logging;
using System;
using System.ComponentModel;
using System.Threading;

namespace KaynakMakinesi.UI
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly ISettingsStore<AppSettings> _settingsStore;
        private readonly InMemoryLogSink _logSink;
        private readonly IConnectionSupervisor _conn;
        private readonly IAppLogger _log;
        private readonly IModbusService _modbusService;
        private const int SummaryLogCount = 30; // listbox’ta tutacağımız
        public MainForm(
            ISettingsStore<AppSettings> settingsStore, 
            InMemoryLogSink logSink, 
            IConnectionSupervisor conn,
           
            IAppLogger log,
             IModbusService modbusService)
        {
            #region Designer
            InitializeComponent();
            _settingsStore = settingsStore;
            _logSink = logSink;
            _conn = conn;
            _log = log;
            _modbusService = modbusService ?? throw new ArgumentNullException(nameof(modbusService));

            // Başlangıç
            UpdatePlcTargetFromSettings();
            UpdateConnUi(_conn.State, "Başladı");

            // Event’ler
            _conn.StateChanged += Conn_StateChanged;
            _logSink.EntryAdded += LogSink_EntryAdded;
            _settingsStore.SettingsChanged += (s, e) => BeginInvoke((Action)UpdatePlcTargetFromSettings);

            // İlk log snapshot (özet liste)
            foreach (var e in _logSink.Snapshot())
                AddLogToSummary(e);

            // Dokunmatik nav
            //tileItemSettings.ItemClick += (s, e) => OpenSettings();
            //tileItemLogs.ItemClick += (s, e) => OpenLogs();
            #endregion

        }

        private void UpdateConnUi(ConnectionState state, string reason)
        {
            lblConnState.Text = state.ToString();
            lblConnReason.Text = reason ?? "-";

            if (state == ConnectionState.Connected)
                lblLastOk.Text = "Last OK: " + DateTime.Now.ToString("HH:mm:ss");

            // İkonu state’e göre değiştir (istersen renk de verirsin)
            // svgConn.SvgImage = ... (projene bir-iki svg ekle, burada seç)
        }

        private void UpdatePlcTargetFromSettings()
        {
            var s = _settingsStore.Load();
            lblPlcTarget.Text = $"{s.Plc.Ip}:{s.Plc.Port} (Unit:{s.Plc.UnitId})";
        }

        private void LogSink_EntryAdded(object sender, LogEntry e)
        {
            if (IsDisposed) return;
            BeginInvoke((Action)(() => AddLogToSummary(e)));
        }

        private void AddLogToSummary(LogEntry e)
        {
            // Dokunmatik için kısa satır:
            var line = $"{e.Timestamp:HH:mm:ss} [{e.Level}] {e.Source} - {e.Message}";

            listLogs.Items.Add(line);

            while (listLogs.ItemCount > SummaryLogCount)
                listLogs.Items.RemoveAt(0);

            // İstersen en alta otomatik kaydır:
            listLogs.TopIndex = Math.Max(0, listLogs.ItemCount - 1);
        }

        private void btnAyarlar_Click(object sender, EventArgs e)
        {
            using (var f = new SettingsForm(_settingsStore))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    _log.Info(nameof(MainForm), "Ayarlar kaydedildi.");
            }
        }

        private void Conn_StateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (IsDisposed) return;
            BeginInvoke((Action)(() => UpdateConnUi(e.State, e.Reason)));
        }
       
        private  void MainForm_Load(object sender, EventArgs e)
        {
        }

        private async void btnOku_Click(object sender, EventArgs e)
        {
            try
            {
                var ok = await _modbusService.ReadAutoAsync(
        "10001",
        System.Threading.CancellationToken.None);

                lblConnState.Text = ok.Success ? ok.Value.ToString() : "Yazma başarısız";
            }
            catch (Exception ex)
            {
                lblConnState.Text = "Kritik: " + ex.Message;
                
            }
        }
    }
}
