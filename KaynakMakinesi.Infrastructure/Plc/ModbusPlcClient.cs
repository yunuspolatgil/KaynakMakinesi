using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc;
using Modbus.Device;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KaynakMakinesi.Infrastructure.Plc
{
    public sealed class ModbusPlcClient : IPlcClient
    {
        // NModbus master thread-safe değil => tüm IO seri
        private readonly SemaphoreSlim _io = new SemaphoreSlim(1, 1);

        private readonly IAppLogger _log;
        private TcpClient _tcp;
        private IModbusMaster _master;

        public ModbusPlcClient(IAppLogger log = null)
        {
            _log = log;
        }

        public bool IsConnected => _master != null;

        public async Task<bool> TryConnectAsync(string ip, int port, int timeoutMs, CancellationToken ct)
        {
            await _io.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                ct.ThrowIfCancellationRequested();

                Cleanup_NoThrow();

                var tcp = new TcpClient
                {
                    ReceiveTimeout = timeoutMs,
                    SendTimeout = timeoutMs
                };

                // timeout’lu connect
                var connectTask = tcp.ConnectAsync(ip, port);
                var finished = await Task.WhenAny(connectTask, Task.Delay(timeoutMs, ct)).ConfigureAwait(false);

                if (finished != connectTask || !tcp.Connected)
                {
                    try { tcp.Close(); } catch { }
                    try { tcp.Dispose(); } catch { }
                    return false;
                }

                var master = ModbusIpMaster.CreateIp(tcp);
                master.Transport.Retries = 0;

                _tcp = tcp;
                _master = master;
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (SocketException ex) { try { _log?.Error(nameof(ModbusPlcClient), "Bağlantıda soket hatası", ex); } catch { } Cleanup_NoThrow(); return false; }
            catch (Exception ex) { try { _log?.Error(nameof(ModbusPlcClient), "Bağlantı hatası", ex); } catch { } Cleanup_NoThrow(); return false; }
            finally
            {
                _io.Release();
            }
        }

        public async Task DisconnectAsync(CancellationToken ct)
        {
            await _io.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                Cleanup_NoThrow();
            }
            finally
            {
                _io.Release();
            }
        }

        public Task<bool[]> ReadCoilsAsync(byte unitId, ushort start0, ushort count, CancellationToken ct)
            => ExecuteReadAsync(m => m.ReadCoils(unitId, start0, count), ct);

        public Task<bool[]> ReadDiscreteInputsAsync(byte unitId, ushort start0, ushort count, CancellationToken ct)
            => ExecuteReadAsync(m => m.ReadInputs(unitId, start0, count), ct);

        public Task<ushort[]> ReadInputRegistersAsync(byte unitId, ushort start0, ushort count, CancellationToken ct)
            => ExecuteReadAsync(m => m.ReadInputRegisters(unitId, start0, count), ct);

        public Task<ushort[]> ReadHoldingRegistersAsync(byte unitId, ushort start0, ushort count, CancellationToken ct)
            => ExecuteReadAsync(m => m.ReadHoldingRegisters(unitId, start0, count), ct);

        public Task WriteSingleCoilAsync(byte unitId, ushort address0, bool value, CancellationToken ct)
            => ExecuteWriteAsync(m => m.WriteSingleCoil(unitId, address0, value), ct);

        public Task WriteSingleRegisterAsync(byte unitId, ushort address0, ushort value, CancellationToken ct)
            => ExecuteWriteAsync(m => m.WriteSingleRegister(unitId, address0, value), ct);

        public Task WriteMultipleRegistersAsync(byte unitId, ushort start0, ushort[] values, CancellationToken ct)
            => ExecuteWriteAsync(m => m.WriteMultipleRegisters(unitId, start0, values), ct);

        // NOTE: to guarantee the application never crashes because of PLC IO,
        // we swallow all non-cancellation exceptions here and return safe defaults.
        private async Task<T> ExecuteReadAsync<T>(Func<IModbusMaster, T> read, CancellationToken ct)
        {
            await _io.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                ct.ThrowIfCancellationRequested();
                var master = _master;
                if (master == null)
                {
                    // return safe empty arrays for common types to avoid nulls
                    if (typeof(T) == typeof(bool[])) return (T)(object)Array.Empty<bool>();
                    if (typeof(T) == typeof(ushort[])) return (T)(object)Array.Empty<ushort>();
                    return default(T); // no connection => safe default
                }

                try
                {
                    // sync IO => UI bloklamasın
                    return await Task.Run(() =>
                    {
                        try
                        {
                            return read(master);
                        }
                        catch (Exception ex)
                        {
                            try { _log?.Error(nameof(ModbusPlcClient), "Okuma hatası, bağlantı temizleniyor.", ex); } catch { }

                            // Eğer iletişimle ilgili bir hata ise bağlantıyı temizle
                            if (IsCommException(ex))
                                Cleanup_NoThrow();

                            // swallow and return safe default to avoid throwing
                            if (typeof(T) == typeof(bool[])) return (T)(object)Array.Empty<bool>();
                            if (typeof(T) == typeof(ushort[])) return (T)(object)Array.Empty<ushort>();
                            return default(T);
                        }
                    }, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // propagate cancellation
                    throw;
                }
                catch (Exception ex)
                {
                    try { _log?.Error(nameof(ModbusPlcClient), "Sarmalayıcı hatası okundu, bağlantı temizleniyor", ex); } catch { }

                    // any unexpected error: ensure connection is cleaned and return default
                    try { Cleanup_NoThrow(); } catch { }

                    if (typeof(T) == typeof(bool[])) return (T)(object)Array.Empty<bool>();
                    if (typeof(T) == typeof(ushort[])) return (T)(object)Array.Empty<ushort>();
                    return default(T);
                }
            }
            finally
            {
                _io.Release();
            }
        }

        private async Task ExecuteWriteAsync(Action<IModbusMaster> write, CancellationToken ct)
        {
            await _io.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                ct.ThrowIfCancellationRequested();

                var master = _master;
                if (master == null)
                    return; // no connection => nothing to do

                try
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            write(master);
                        }
                        catch (Exception ex)
                        {
                            try { _log?.Error(nameof(ModbusPlcClient), "Yazma hatası, bağlantı temizleniyor.", ex); } catch { }

                            if (IsCommException(ex))
                                Cleanup_NoThrow();

                            // swallow
                        }
                    }, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    try { _log?.Error(nameof(ModbusPlcClient), "Yazma sarmalayıcı hatası, bağlantı temizleniyor.", ex); } catch { }
                    try { Cleanup_NoThrow(); } catch { }
                    // swallow all non-cancellation exceptions to prevent crashes
                }
            }
            finally
            {
                _io.Release();
            }
        }

        private static bool IsCommException(Exception ex)
        {
            // Bağlantı kopmalarında görülen tipik exceptionlar
            return ex is SocketException
                || ex is IOException
                || ex is ObjectDisposedException
                || ex is TimeoutException;
        }

        private void Cleanup_NoThrow()
        {
            try { _master?.Dispose(); } catch { }
            _master = null;

            try { _tcp?.Close(); } catch { }
            try { _tcp?.Dispose(); } catch { }
            _tcp = null;
        }

        public void Dispose()
        {
            try { _io.Wait(250); } catch { }
            try { Cleanup_NoThrow(); } catch { }
            try { _io.Release(); } catch { }
            try { _io.Dispose(); } catch { }
        }
    }
}
