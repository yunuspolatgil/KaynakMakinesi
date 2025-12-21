using System;
using System.Threading;
using System.Threading.Tasks;

namespace KaynakMakinesi.Core.Plc
{
    public enum ConnectionState { Disconnected, Connecting, Connected, Faulted }

    public sealed class ConnectionStateChangedEventArgs : EventArgs
    {
        public ConnectionState State { get; }
        public string Reason { get; }
        public ConnectionStateChangedEventArgs(ConnectionState state, string reason)
        {
            State = state;
            Reason = reason;
        }
    }

    public interface IPlcClient : IDisposable
    {
        bool IsConnected { get; }
        Task ConnectAsync(string ip, int port, int timeoutMs, CancellationToken ct);
        Task DisconnectAsync(CancellationToken ct);

        // Modbus örnekleri
        Task<ushort[]> ReadHoldingRegistersAsync(byte unitId, ushort startAddress, ushort numberOfPoints, CancellationToken ct);
        Task WriteSingleRegisterAsync(byte unitId, ushort address, ushort value, CancellationToken ct);
    }

    public interface IConnectionSupervisor
    {
        ConnectionState State { get; }
        event EventHandler<ConnectionStateChangedEventArgs> StateChanged;

        void Start();
        void Stop();
    }
}