using System;
using System.Collections.Generic;
using System.Linq;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Plc.Profile;
using ValueType = KaynakMakinesi.Core.Plc.ValueType;

namespace KaynakMakinesi.Infrastructure.Plc.Profile
{
    /// <summary>
    /// GMT PLC 496T profili - Gerçek operand tablosuna göre
    /// </summary>
    public sealed class Gmt496Profile : IPlcProfile
    {
        public string Name => "GMT PLC 496T";

        private static readonly HashSet<int> _integerAddresses = new HashSet<int>
        {
            42001, 42003, 42005, 42007, 42009, 42011, 42013, 42015, 42021, 42023
        };

        private static readonly HashSet<int> _realAddresses = new HashSet<int>
        {
            42017, 42019, 42025, 42027, 42029
        };

        private readonly List<ProfileRule> _rules = new List<ProfileRule>
        {
            new ProfileRule
            { 
                From1Based = 40000, 
                To1Based = 49999, 
                Area = ModbusArea.HoldingRegister, 
                Type = ValueType.UShort, 
                Length = 1, 
                ReadOnly = false, 
                Step = 1 
            },
            new ProfileRule
            { 
                From1Based = 1,  // ÖNEMLİ: Coil adresleri 1-based (1-9999)
                To1Based = 9999, 
                Area = ModbusArea.Coil, 
                Type = ValueType.Bool, 
                Length = 1, 
                ReadOnly = false, 
                Step = 1 
            },
            new ProfileRule
            { 
                From1Based = 10000, 
                To1Based = 19999, 
                Area = ModbusArea.DiscreteInput, 
                Type = ValueType.Bool, 
                Length = 1, 
                ReadOnly = true, 
                Step = 1 
            },
            new ProfileRule
            { 
                From1Based = 30000, 
                To1Based = 39999, 
                Area = ModbusArea.InputRegister, 
                Type = ValueType.UShort, 
                Length = 1, 
                ReadOnly = true, 
                Step = 1 
            },
        };

        public IReadOnlyList<ProfileRule> Rules => _rules;

        public int GetHumanBase1Based(ModbusArea area)
        {
            // Modbus standartlarına göre base adresler (1-based human readable)
            // Holding Register: 40001-49999 aralığı (40000 base + 1-based offset)
            // Discrete Input: 10001-19999 aralığı (10000 base + 1-based offset)
            // Input Register: 30001-39999 aralığı (30000 base + 1-based offset)
            // Coil: 1-9999 aralığı (1 base + 0-based offset, ama 1-based gösterilir)
            switch (area)
            {
                case ModbusArea.Coil: return 1;  // Coil adresleri 1'den başlar (1-based)
                case ModbusArea.DiscreteInput: return 10000;
                case ModbusArea.InputRegister: return 30000;
                case ModbusArea.HoldingRegister: return 40000;
                default: return 0;
            }
        }

        public bool TryResolveByModbusAddress(int address1Based, out ResolvedAddress resolved, out string error)
        {
            resolved = null;
            error = null;

            var rule = _rules.FirstOrDefault(r => address1Based >= r.From1Based && address1Based <= r.To1Based);
            if (rule == null)
            {
                error = $"Adres {address1Based} geçerli aralıkta değil.";
                return false;
            }

            var baseAddress = GetHumanBase1Based(rule.Area);
            ushort start0 = (ushort)(address1Based - baseAddress);

            ValueType finalType = rule.Type;
            ushort finalLength = rule.Length;

            if (rule.Area == ModbusArea.HoldingRegister)
            {
                if (_integerAddresses.Contains(address1Based))
                {
                    finalType = ValueType.Int32;
                    finalLength = 2;
                }
                else if (_realAddresses.Contains(address1Based))
                {
                    finalType = ValueType.Float;
                    finalLength = 2;
                }
            }

            if (start0 > 65535)
            {
                error = $"Start0={start0} çok büyük";
                return false;
            }

            resolved = new ResolvedAddress
            {
                Area = rule.Area,
                Type = finalType,
                Length = finalLength,
                ReadOnly = rule.ReadOnly,
                Start0 = start0,
                HumanAddress1Based = address1Based
            };

            return true;
        }

        public bool TryResolveByOperand(string operand, out ResolvedAddress resolved, out string error)
        {
            resolved = null;
            error = null;

            if (string.IsNullOrWhiteSpace(operand))
            {
                error = "Operand boş.";
                return false;
            }

            operand = operand.Trim().ToUpperInvariant();

            if (operand.Length < 3)
            {
                error = "Operand formatı geçersiz.";
                return false;
            }

            string prefix = operand.Substring(0, 2);
            if (!int.TryParse(operand.Substring(2), out int idx) || idx < 0)
            {
                error = "Operand index geçersiz.";
                return false;
            }

            int address1Based;

            switch (prefix)
            {
                case "MW":
                    address1Based = 40000 + idx;  // MW0 -> 40000
                    break;
                case "MI":
                    address1Based = 42000 + (idx * 2);  // MI0 -> 42000, MI1 -> 42002
                    break;
                case "MF":
                    var floatAddresses = new[] { 42017, 42019, 42025, 42027, 42029 };
                    if (idx >= 0 && idx < floatAddresses.Length)
                    {
                        address1Based = floatAddresses[idx];
                    }
                    else
                    {
                        error = $"MF{idx} tanımlı değil. Geçerli: MF0-MF4";
                        return false;
                    }
                    break;
                case "MB":
                    // MB0 -> Coil 1 (çünkü Coil adresleri 1-based)
                    // MB1 -> Coil 2
                    address1Based = 1 + idx;  // MB0 -> 1, MB1 -> 2
                    break;
                case "IP":
                    address1Based = 10000 + idx;  // IP0 -> 10000
                    break;
                case "QP":
                    // QP (Output Coils) de 1-based
                    address1Based = 1 + idx;  // QP0 -> 1, QP1 -> 2
                    break;
                case "IW":
                    address1Based = 30000 + idx;  // IW0 -> 30000
                    break;
                default:
                    error = $"Bilinmeyen prefix: {prefix}. Geçerli: MW, MI, MF, MB, IP, QP, IW";
                    return false;
            }

            return TryResolveByModbusAddress(address1Based, out resolved, out error);
        }
    }
}
