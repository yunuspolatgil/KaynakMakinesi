using KaynakMakinesi.Core.Plc;
using Modbus.Device;

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KaynakMakinesi.Infrastructure.Plc
{
    public sealed class ModbusPlcClient : IPlcClient
    {
        private readonly object _sync = new object();
        private readonly System.Threading.SemaphoreSlim _gate = new System.Threading.SemaphoreSlim(1, 1);
        private TcpClient _tcp;
        private IModbusMaster _master;

        public bool IsConnected
        {
            get
            {
                lock (_sync) return _tcp != null && _tcp.Connected && _master != null;
            }
        }

        public async Task<bool> TryConnectAsync(string ip, int port, int timeoutMs, CancellationToken ct)
        {
            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                ct.ThrowIfCancellationRequested();

                lock (_sync) { Cleanup_NoThrow(); }

                var tcp = new System.Net.Sockets.TcpClient
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
            catch (System.Net.Sockets.SocketException) { return false; }
            catch { lock (_sync) { Cleanup_NoThrow(); } return false; }
            finally { _gate.Release(); }
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

        public Task<ushort[]> ReadHoldingRegistersAsync(byte unitId, ushort startAddress, ushort numberOfPoints, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                lock (_sync)
                {
                    if (_master == null) throw new InvalidOperationException("PLC not connected.");
                    return _master.ReadHoldingRegisters(unitId, startAddress, numberOfPoints);
                }
            }, ct);
        }

        public Task WriteSingleRegisterAsync(byte unitId, ushort address, ushort value, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                lock (_sync)
                {
                    if (_master == null) throw new InvalidOperationException("PLC not connected.");
                    _master.WriteSingleRegister(unitId, address, value);
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
