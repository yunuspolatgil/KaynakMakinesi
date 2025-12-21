using System;
using System.Threading;
using System.Threading.Tasks;
using KaynakMakinesi.Core.Abstractions;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Settings;

namespace KaynakMakinesi.Infrastructure.Plc
{
    public sealed class PlcConnectionSupervisor : IConnectionSupervisor
    {
        private readonly ISettingsStore<AppSettings> _settingsStore;
        private readonly IPlcClient _plc;
        private readonly IAppLogger _log;

        private readonly object _lock = new object();
        private CancellationTokenSource _cts;
        private Task _loop;

        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
        public event EventHandler<ConnectionStateChangedEventArgs> StateChanged;

        public PlcConnectionSupervisor(ISettingsStore<AppSettings> settingsStore, IPlcClient plc, IAppLogger log)
        {
            _settingsStore = settingsStore;
            _plc = plc;
            _log = log;

            _settingsStore.SettingsChanged += (s, e) =>
            {
                _log.Info(nameof(PlcConnectionSupervisor), "Ayar değişti: PLC bağlantısı yeniden kurulacak.");
                ForceReconnect();
            };
        }

        public void Start()
        {
            lock (_lock)
            {
                if (_cts != null) return;
                _cts = new CancellationTokenSource();
                _loop = Task.Run(() => RunAsync(_cts.Token));
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_cts == null) return;
                _cts.Cancel();
                _cts = null;
            }
        }

        private void SetState(ConnectionState state, string reason)
        {
            State = state;
            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(state, reason));
        }

        private async Task RunAsync(CancellationToken ct)
        {
            int backoffMs = 500;

            while (!ct.IsCancellationRequested)
            {
                var s = _settingsStore.Load();
                var plc = s.Plc;

                try
                {
                    if (!_plc.IsConnected)
                    {
                        SetState(ConnectionState.Connecting, "Bağlanıyor...");
                        _log.Info(nameof(PlcConnectionSupervisor), $"PLC bağlanıyor: {plc.Ip}:{plc.Port}");

                        await _plc.ConnectAsync(plc.Ip, plc.Port, plc.TimeoutMs, ct);
                        SetState(ConnectionState.Connected, "Bağlandı");
                        _log.Info(nameof(PlcConnectionSupervisor), "PLC bağlantısı OK");

                        backoffMs = 500; // başarı -> backoff reset
                    }

                    // Heartbeat probe (asıl “anlık takip” kısmı)
                    var _ = await _plc.ReadHoldingRegistersAsync(plc.UnitId, plc.HeartbeatAddress, 1, ct);
                    await Task.Delay(plc.HeartbeatIntervalMs, ct);
                }
                catch (Exception ex)
                {
                    _log.Error(nameof(PlcConnectionSupervisor), "PLC bağlantı/probe hatası", ex);

                    try { await _plc.DisconnectAsync(ct); } catch { }

                    SetState(ConnectionState.Disconnected, "Koptu / Hata");
                    await Task.Delay(backoffMs, ct);

                    // kontrollü backoff (çok hızlı deneme PLC’yi de ağı da boğar)
                    backoffMs = Math.Min(backoffMs * 2, 10000);
                }
            }

            try { await _plc.DisconnectAsync(CancellationToken.None); } catch { }
            SetState(ConnectionState.Disconnected, "Durduruldu");
        }

        private void ForceReconnect()
        {
            // En güvenlisi: disconnect + supervisor loop zaten yeniden bağlar
            Task.Run(async () =>
            {
                try { await _plc.DisconnectAsync(CancellationToken.None); } catch { }
            });
        }
    }
}
