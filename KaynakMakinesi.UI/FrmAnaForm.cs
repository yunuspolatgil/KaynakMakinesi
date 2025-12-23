using DevExpress.XtraBars;
using KaynakMakinesi.Core.Abstractions;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.Core.Settings;
using KaynakMakinesi.Infrastructure.Logging;
using KaynakMakinesi.Infrastructure.Tags;
using KaynakMakinesi.UI.Utils;
using System;
using ConnectionState = KaynakMakinesi.Core.Plc.ConnectionState;

namespace KaynakMakinesi.UI
{
    public partial class FrmAnaForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private readonly ISettingsStore<AppSettings> _settingsStore;
        private readonly InMemoryLogSink _logSink;
        private readonly IConnectionSupervisor _conn;
        private readonly SqliteTagRepository _tagRepo;
        private readonly IAppLogger _log;
        private readonly IModbusService _modbusService;
        private const int SummaryLogCount = 30; // listbox’ta tutacağımız
        public FrmAnaForm
            (
                ISettingsStore<AppSettings> settingsStore,
                InMemoryLogSink logSink,
                IConnectionSupervisor conn,
                IAppLogger log,
                IModbusService modbusService,
                SqliteTagRepository tagRepo
            )
        {
            InitializeComponent();
            _settingsStore = settingsStore;
            _logSink = logSink;
            _conn = conn;
            _log = log;
            _tagRepo = tagRepo;
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
            FrmAcilisForm mainForm = new FrmAcilisForm();
            mainForm.MdiParent = this;
            mainForm.Show();
        }

        private void UpdateConnUi(ConnectionState state, string reason)
        {
            lblConnState.Caption = state.ToString();
            lblConnReason.Caption = reason ?? "-";

            if (state == ConnectionState.Connected)
                lblLastOk.Caption = DateTime.Now.ToString("HH:mm:ss");

            // İkonu state’e göre değiştir (istersen renk de verirsin)
            // svgConn.SvgImage = ... (projene bir-iki svg ekle, burada seç)
        }

        private void UpdatePlcTargetFromSettings()
        {
            var s = _settingsStore.Load();
            lblPlcTarget.Caption = $"{s.Plc.Ip}:{s.Plc.Port} (Slave:{s.Plc.UnitId})";
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



        private void Conn_StateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (IsDisposed) return;
            BeginInvoke((Action)(() => UpdateConnUi(e.State, e.Reason)));
        }

        private void btnTagYonetim_ItemClick(object sender, ItemClickEventArgs e)
        {
            UiHelpers.ShowMdiSingleton(this,
                factory: () => new FrmTagManager(_tagRepo, _modbusService, _log),
                text: "Tag Yönetimi",
                maximize: true);
        }

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var f = new SettingsForm(_settingsStore))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    _log.Info(nameof(FrmAcilisForm), "Ayarlar kaydedildi.");
            }
        }

        private void btnSagTorcKalibrasyon_ItemClick(object sender, ItemClickEventArgs e)
        {
            FrmTorcSag frmTorcSag = new FrmTorcSag(_modbusService,_log);
            frmTorcSag.ShowDialog();
        }
    }
}