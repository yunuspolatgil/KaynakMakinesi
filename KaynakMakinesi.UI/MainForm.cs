using KaynakMakinesi.Core.Abstractions;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Settings;
using KaynakMakinesi.Infrastructure.Logging;
using System;
using System.ComponentModel;

namespace KaynakMakinesi.UI
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly ISettingsStore<AppSettings> _settingsStore;
        private readonly InMemoryLogSink _logSink;
        private readonly IConnectionSupervisor _conn;
        private readonly IAppLogger _log;

        private readonly BindingList<LogEntry> _logList = new BindingList<LogEntry>();
        public MainForm(ISettingsStore<AppSettings> settingsStore, InMemoryLogSink logSink, IConnectionSupervisor conn, IAppLogger log)
        {
            InitializeComponent();
            _settingsStore = settingsStore;
            _logSink = logSink;
            _conn = conn;
            _log = log;

            // gridControl1.DataSource = _logList;  // DevExpress GridControl
            // statusLabelConnection.Text = "...";

            _logSink.EntryAdded += OnLogEntryAdded;
            _conn.StateChanged += OnConnStateChanged;

            // İlk snapshot (program açılınca son loglar görünsün)
            foreach (var e in _logSink.Snapshot())
                _logList.Add(e);
        }

        private void OnLogEntryAdded(object sender, LogEntry e)
        {
            if (IsDisposed) return;
            BeginInvoke((Action)(() =>
            {
                _logList.Add(e);
                while (_logList.Count > 2000) _logList.RemoveAt(0);
            }));
        }

        private void OnConnStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (IsDisposed) return;
            BeginInvoke((Action)(() =>
            {
                // statusLabelConnection.Text = $"{e.State} - {e.Reason}";
            }));
        }
    }
}
