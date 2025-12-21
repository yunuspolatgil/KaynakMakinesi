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

        private TcpClient _tcp;
        private IModbusMaster _master;

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
            catch (SocketException) { Cleanup_NoThrow(); return false; }
            catch { Cleanup_NoThrow(); return false; }
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

        private async Task<T> ExecuteReadAsync<T>(Func<IModbusMaster, T> read, CancellationToken ct)
        {
            await _io.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                ct.ThrowIfCancellationRequested();

                var master = _master;
                if (master == null)
                    throw new InvalidOperationException("PLC bağlantısı yok.");

                // sync IO => UI bloklamasın
                return await Task.Run(() => read(master), ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (IsCommException(ex))
            {
                Cleanup_NoThrow();
                throw new InvalidOperationException("PLC bağlantısı koptu.", ex);
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
                    throw new InvalidOperationException("PLC bağlantısı yok.");

                await Task.Run(() => write(master), ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (IsCommException(ex))
            {
                Cleanup_NoThrow();
                throw new InvalidOperationException("PLC bağlantısı koptu.", ex);
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
                || ex is ObjectDisposedException;
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
