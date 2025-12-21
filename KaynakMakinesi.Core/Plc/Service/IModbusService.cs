using System.Threading;
using System.Threading.Tasks;

namespace KaynakMakinesi.Core.Plc.Service
{
    public sealed class ModbusReadResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public object Value { get; set; }
        public Plc.ResolvedAddress Address { get; set; }
    }

    public interface IModbusService
    {
        Task<ModbusReadResult> ReadAutoAsync(string input, CancellationToken ct);
        Task<bool> WriteAutoAsync(string input, object value, CancellationToken ct);
        Task<bool> WriteTextAsync(string input, string valueText, CancellationToken ct);
    }
}