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

                // Run the supervisor in a resilient background task so an unexpected exception
                // inside RunAsync will not fault the task and bring down the process.
                var task = Task.Run(async () =>
                {
                    var ct = _cts.Token;
                    while (!ct.IsCancellationRequested)
                    {
                        try
                        {
                            await RunAsync(ct).ConfigureAwait(false);
                            // RunAsync normally only returns when cancelled; exit loop
                            break;
                        }
                        catch (OperationCanceledException) when (ct.IsCancellationRequested)
                        {
                            // normal shutdown
                            break;
                        }
                        catch (Exception ex)
                        {
                            // Log and retry after short delay (resilient)
                            try { _log.Error(nameof(PlcConnectionSupervisor), "Supervisor crashed, will restart", ex); } catch { }

                            try { await Task.Delay(1000, CancellationToken.None).ConfigureAwait(false); } catch { }
                            // loop continues and RunAsync will be restarted
                        }
                    }
                });

                // Observe potential faults to avoid UnobservedTaskException
                _loop = task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        try { _log.Error(nameof(PlcConnectionSupervisor), "Supervisor background task faulted", t.Exception); } catch { }
                    }
                }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
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

            try { _loop?.Wait(500); } catch { }
            _loop = null;
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

                        var ok = await _plc.TryConnectAsync(plc.Ip, plc.Port, plc.TimeoutMs, ct).ConfigureAwait(false);
                        if (!ok)
                        {
                            SetState(ConnectionState.Disconnected, "Bağlanamadı");
                            await Task.Delay(backoffMs, ct).ConfigureAwait(false);
                            backoffMs = Math.Min(backoffMs * 2, 10000);
                            continue;
                        }

                        SetState(ConnectionState.Connected, "Bağlandı");
                        backoffMs = 500;
                        SetState(ConnectionState.Connected, "Bağlandı");
                        _log.Info(nameof(PlcConnectionSupervisor), "PLC bağlantısı OK");

                        backoffMs = 500; // başarı -> backoff reset
                    }

                    // Heartbeat probe (asıl “anlık takip” kısmı)
                    var _ = await _plc.ReadHoldingRegistersAsync(plc.UnitId, plc.HeartbeatAddress, 1, ct).ConfigureAwait(false);
                    await Task.Delay(plc.HeartbeatIntervalMs, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // cancellation requested -> break loop
                    break;
                }
                catch (Exception ex)
                {
                    _log.Error(nameof(PlcConnectionSupervisor), "PLC bağlantı/probe hatası", ex);

                    try { await _plc.DisconnectAsync(ct).ConfigureAwait(false); } catch { }

                    SetState(ConnectionState.Disconnected, "Koptu / Hata");
                    try { await Task.Delay(backoffMs, ct).ConfigureAwait(false); } catch { }

                    // kontrollü backoff (çok hızlı deneme PLC’yi de ağı da boğar)
                    backoffMs = Math.Min(backoffMs * 2, 10000);
                }
            }

            try { await _plc.DisconnectAsync(CancellationToken.None).ConfigureAwait(false); } catch { }
            SetState(ConnectionState.Disconnected, "Durduruldu");
        }

        private void ForceReconnect()
        {
            // En güvenlisi: disconnect + supervisor loop zaten yeniden bağlar
            Task.Run(async () =>
            {
                try { await _plc.DisconnectAsync(CancellationToken.None).ConfigureAwait(false); } catch { }
            }).ContinueWith(t =>
            {
                if (t.IsFaulted) try { _log.Error(nameof(PlcConnectionSupervisor), "ForceReconnect background task faulted", t.Exception); } catch { }
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
