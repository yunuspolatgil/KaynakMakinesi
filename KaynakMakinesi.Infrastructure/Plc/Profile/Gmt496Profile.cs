using System;
using System.Collections.Generic;
using System.Linq;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Plc.Profile;
using ValueType = KaynakMakinesi.Core.Plc.ValueType;

namespace KaynakMakinesi.Infrastructure.Plc.Profile
{
    public sealed class Gmt496Profile : IPlcProfile
    {
        public string Name => "GMT PLC 496T";

        private readonly List<ProfileRule> _rules = new List<ProfileRule>
        {
            // MW0..MW511 => 40001..40512 (1 word)
            new ProfileRule{ From1Based=40001, To1Based=40512, Area=ModbusArea.HoldingRegister, Type=ValueType.UShort, Length=1, ReadOnly=false, Step=1 },

            // MB0..MB1023 => 00001..01024 (coil bit)
            new ProfileRule{ From1Based=1, To1Based=1024, Area=ModbusArea.Coil, Type=ValueType.Bool, Length=1, ReadOnly=false, Step=1 },

            // IP0..IP272 => 10001..10273 (discrete input)
            new ProfileRule{ From1Based=10001, To1Based=10273, Area=ModbusArea.DiscreteInput, Type=ValueType.Bool, Length=1, ReadOnly=true, Step=1 },

            // QP0..QP272 => 02001..02273 (coil bit - output)
            new ProfileRule{ From1Based=2001, To1Based=2273, Area=ModbusArea.Coil, Type=ValueType.Bool, Length=1, ReadOnly=false, Step=1 },

            // MI0..MI1023 => 41001..43047 (2 word int32)
            new ProfileRule{ From1Based=41001, To1Based=43047, Area=ModbusArea.HoldingRegister, Type=ValueType.Int32, Length=2, ReadOnly=false, Step=2 },

            // MF0..MF1023 => 44001..46047 (2 word float)
            new ProfileRule{ From1Based=44001, To1Based=46047, Area=ModbusArea.HoldingRegister, Type=ValueType.Float, Length=2, ReadOnly=false, Step=2 },
        };

        public IReadOnlyList<ProfileRule> Rules => _rules;

        public int GetHumanBase1Based(ModbusArea area)
        {
            switch (area)
            {
                case ModbusArea.Coil: return 1;
                case ModbusArea.DiscreteInput: return 10001;
                case ModbusArea.InputRegister: return 30001;
                case ModbusArea.HoldingRegister: return 40001;
                default: return 1;
            }
        }

        public bool TryResolveByModbusAddress(int address1Based, out ResolvedAddress resolved, out string error)
        {
            resolved = null;
            error = null;

            var rule = _rules.FirstOrDefault(r => address1Based >= r.From1Based && address1Based <= r.To1Based);
            if (rule == null)
            {
                error = "Adres profile aralıklarında değil.";
                return false;
            }

            // 2-word tiplerde hizalama kontrolü (MI/MF)
            if (rule.Step == 2)
            {
                var diff = address1Based - rule.From1Based;
                if (diff % 2 != 0)
                {
                    error = "Adres hizası hatalı (2 word tip). Örn: 41001,41003,... şeklinde olmalı.";
                    return false;
                }
            }

            var base1 = GetHumanBase1Based(rule.Area);
            var start0 = address1Based - base1;
            if (start0 < 0 || start0 > ushort.MaxValue)
            {
                error = "Adres Start0 aralığı geçersiz.";
                return false;
            }

            resolved = new ResolvedAddress
            {
                Area = rule.Area,
                Type = rule.Type,
                Length = rule.Length,
                ReadOnly = rule.ReadOnly,
                Start0 = (ushort)start0,
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

            // MW12, MI10, MF3, MB7, IP0, QP2
            string prefix;
            int idx;

            // basit parse: ilk 2 harf prefix, kalan sayı
            if (operand.Length < 3)
            {
                error = "Operand formatı geçersiz.";
                return false;
            }

            prefix = operand.Substring(0, 2);
            if (!int.TryParse(operand.Substring(2), out idx) || idx < 0)
            {
                error = "Operand index geçersiz.";
                return false;
            }

            int address1Based;

            switch (prefix)
            {
                case "MW": address1Based = 40001 + idx; break;
                case "MB": address1Based = 1 + idx; break;
                case "IP": address1Based = 10001 + idx; break;
                case "QP": address1Based = 2001 + idx; break;
                case "MI": address1Based = 41001 + (idx * 2); break;
                case "MF": address1Based = 44001 + (idx * 2); break;
                default:
                    error = "Bilinmeyen operand prefix.";
                    return false;
            }

            return TryResolveByModbusAddress(address1Based, out resolved, out error);
        }
    }
}
