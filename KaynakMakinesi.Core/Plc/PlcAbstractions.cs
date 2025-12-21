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

        // throw etmeyen bağlantı (senin crash olmama kuralın)
        Task<bool> TryConnectAsync(string ip, int port, int timeoutMs, CancellationToken ct);
        Task DisconnectAsync(CancellationToken ct);

        // Bit alanları
        Task<bool[]> ReadCoilsAsync(byte unitId, ushort start0, ushort count, CancellationToken ct);
        Task<bool[]> ReadDiscreteInputsAsync(byte unitId, ushort start0, ushort count, CancellationToken ct);
        Task WriteSingleCoilAsync(byte unitId, ushort address0, bool value, CancellationToken ct);

        // Register alanları
        Task<ushort[]> ReadHoldingRegistersAsync(byte unitId, ushort start0, ushort count, CancellationToken ct);
        Task<ushort[]> ReadInputRegistersAsync(byte unitId, ushort start0, ushort count, CancellationToken ct);
        Task WriteSingleRegisterAsync(byte unitId, ushort address0, ushort value, CancellationToken ct);
        Task WriteMultipleRegistersAsync(byte unitId, ushort start0, ushort[] values, CancellationToken ct);

    }

    public interface IConnectionSupervisor
    {
        ConnectionState State { get; }
        event EventHandler<ConnectionStateChangedEventArgs> StateChanged;

        void Start();
        void Stop();
    }
}