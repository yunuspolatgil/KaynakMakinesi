using System.Collections.Generic;

namespace KaynakMakinesi.Core.Plc.Profile
{
    public interface IPlcProfile
    {
        string Name { get; }

        IReadOnlyList<ProfileRule> Rules { get; }

        // NModbus offset hesabı için: holding base = 40001, input base=30001, coil base=1, di base=10001
        int GetHumanBase1Based(ModbusArea area);

        bool TryResolveByModbusAddress(int address1Based, out Plc.ResolvedAddress resolved, out string error);
        bool TryResolveByOperand(string operand, out Plc.ResolvedAddress resolved, out string error);
    }
}
