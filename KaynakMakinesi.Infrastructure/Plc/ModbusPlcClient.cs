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
        private readonly object _sync = new object();
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        private TcpClient _tcp;
        private IModbusMaster _master;

        public bool IsConnected
        {
            get
            {
                lock (_sync)
                {
                    return _tcp != null && _tcp.Connected && _master != null;
                }
            }
        }

        private static bool IsCommException(Exception ex)
            => ex is IOException
            || ex is SocketException
            || ex is ObjectDisposedException;

        private T WithMaster<T>(string opName, Func<IModbusMaster, T> action)
        {
            lock (_sync)
            {
                if (_master == null)
                    throw new InvalidOperationException("PLC bağlantısı yok.");

                try
                {
                    return action(_master);
                }
                catch (Exception ex) when (IsCommException(ex))
                {
                    // Bağlantı koptu / uzak host kapattı -> iç durumu temizle
                    Cleanup_NoThrow();
                    throw new InvalidOperationException($"PLC iletişim hatası: {opName}. Bağlantı koptu.", ex);
                }
            }
        }

        private void WithMaster(string opName, Action<IModbusMaster> action)
        {
            WithMaster<object>(opName, m => { action(m); return null; });
        }

        public Task<bool[]> ReadCoilsAsync(byte unitId, ushort start0, ushort count, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return WithMaster("ReadCoils", m => m.ReadCoils(unitId, start0, count));
            }, ct);
        }

        public Task<bool[]> ReadDiscreteInputsAsync(byte unitId, ushort start0, ushort count, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return WithMaster("ReadDiscreteInputs", m => m.ReadInputs(unitId, start0, count));
            }, ct);
        }

        public Task<ushort[]> ReadHoldingRegistersAsync(byte unitId, ushort startAddress, ushort numberOfPoints, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return WithMaster("ReadHoldingRegisters", m => m.ReadHoldingRegisters(unitId, startAddress, numberOfPoints));
            }, ct);
        }

        public Task<ushort[]> ReadInputRegistersAsync(byte unitId, ushort start0, ushort count, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return WithMaster("ReadInputRegisters", m => m.ReadInputRegisters(unitId, start0, count));
            }, ct);
        }

        public Task WriteSingleCoilAsync(byte unitId, ushort address0, bool value, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                WithMaster("WriteSingleCoil", m => m.WriteSingleCoil(unitId, address0, value));
            }, ct);
        }

        public Task WriteSingleRegisterAsync(byte unitId, ushort address, ushort value, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                WithMaster("WriteSingleRegister", m => m.WriteSingleRegister(unitId, address, value));
            }, ct);
        }

        public Task WriteMultipleRegistersAsync(byte unitId, ushort start0, ushort[] values, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                WithMaster("WriteMultipleRegisters", m => m.WriteMultipleRegisters(unitId, start0, values));
            }, ct);
        }

        public async Task<bool> TryConnectAsync(string ip, int port, int timeoutMs, CancellationToken ct)
        {
            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                ct.ThrowIfCancellationRequested();

                lock (_sync) { Cleanup_NoThrow(); }

                var tcp = new TcpClient
                {
                    ReceiveTimeout = timeoutMs,
                    SendTimeout = timeoutMs
                };

                var connectTask = tcp.ConnectAsync(ip, port);
                var finished = await Task.WhenAny(connectTask, Task.Delay(timeoutMs, ct)).ConfigureAwait(false);

                if (finished != connectTask || !tcp.Connected)
                {
                    try { tcp.Close(); } catch { }
                    return false;
                }

                // connectTask faulted ise burada patlar -> catch ile false döneceğiz
                await connectTask.ConfigureAwait(false);

                var master = ModbusIpMaster.CreateIp(tcp);
                master.Transport.Retries = 0;

                lock (_sync)
                {
                    _tcp = tcp;
                    _master = master;
                }

                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (SocketException) { return false; }
            catch
            {
                lock (_sync) { Cleanup_NoThrow(); }
                return false;
            }
            finally
            {
                _gate.Release();
            }
        }

        public Task DisconnectAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                lock (_sync)
                {
                    Cleanup_NoThrow();
                }
            }, ct);
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
            lock (_sync) Cleanup_NoThrow();
        }
    }
}
