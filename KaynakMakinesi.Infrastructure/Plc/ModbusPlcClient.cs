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
        private TcpClient _tcp;
        private IModbusMaster _master;

        public bool IsConnected
        {
            get
            {
                lock (_sync) return _tcp != null && _tcp.Connected && _master != null;
            }
        }

        public Task ConnectAsync(string ip, int port, int timeoutMs, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                lock (_sync)
                {
                    Cleanup_NoThrow();

                    _tcp = new TcpClient();
                    _tcp.ReceiveTimeout = timeoutMs;
                    _tcp.SendTimeout = timeoutMs;
                    _tcp.Connect(ip, port);

                    
                    _master = ModbusIpMaster.CreateIp(_tcp);
                    _master.Transport.Retries = 0; // retry'yi biz yöneteceğiz
                }
            }, ct);
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
