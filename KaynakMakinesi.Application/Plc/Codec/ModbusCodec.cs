using System;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Plc.Codec;
using ValueType = KaynakMakinesi.Core.Plc.ValueType;

namespace KaynakMakinesi.Application.Plc.Codec
{
    public sealed class ModbusCodec : IModbusCodec
    {
        public bool SwapWordsFor32Bit { get; set; } = false;

        public object Decode(ValueType type, ushort[] regs)
        {
            if (type == ValueType.Bool)
                throw new InvalidOperationException("Bool decode registers ile yapılmaz.");

            if (regs == null || regs.Length == 0)
                throw new ArgumentException("Register boş.");

            switch (type)
            {
                case ValueType.UShort:
                    return regs[0];

                case ValueType.Int32:
                    EnsureLen(regs, 2);
                    return ToInt32(regs[0], regs[1]);

                case ValueType.Float:
                    EnsureLen(regs, 2);
                    return ToFloat(regs[0], regs[1]);

                default:
                    throw new NotSupportedException();
            }
        }

        public ushort[] Encode(ValueType type, object value)
        {
            switch (type)
            {
                case ValueType.UShort:
                    return new[] { Convert.ToUInt16(value) };

                case ValueType.Int32:
                    {
                        var v = Convert.ToInt32(value);
                        var bytes = BitConverter.GetBytes(v); // little-endian
                        // Modbus register: high byte + low byte => big-endian word kabulü yapıyoruz
                        ushort w1 = (ushort)((bytes[3] << 8) | bytes[2]);
                        ushort w2 = (ushort)((bytes[1] << 8) | bytes[0]);
                        return SwapWordsFor32Bit ? new[] { w2, w1 } : new[] { w1, w2 };
                    }

                case ValueType.Float:
                    {
                        var f = Convert.ToSingle(value);
                        var bytes = BitConverter.GetBytes(f);
                        ushort w1 = (ushort)((bytes[3] << 8) | bytes[2]);
                        ushort w2 = (ushort)((bytes[1] << 8) | bytes[0]);
                        return SwapWordsFor32Bit ? new[] { w2, w1 } : new[] { w1, w2 };
                    }

                default:
                    throw new NotSupportedException("Bool write için coil kullanılmalı.");
            }
        }

        private int ToInt32(ushort r1, ushort r2)
        {
            if (SwapWordsFor32Bit) { var t = r1; r1 = r2; r2 = t; }

            // r1 high word, r2 low word
            var bytes = new byte[4];
            bytes[3] = (byte)(r1 >> 8);
            bytes[2] = (byte)(r1 & 0xFF);
            bytes[1] = (byte)(r2 >> 8);
            bytes[0] = (byte)(r2 & 0xFF);

            return BitConverter.ToInt32(bytes, 0);
        }

        private float ToFloat(ushort r1, ushort r2)
        {
            if (SwapWordsFor32Bit) { var t = r1; r1 = r2; r2 = t; }

            var bytes = new byte[4];
            bytes[3] = (byte)(r1 >> 8);
            bytes[2] = (byte)(r1 & 0xFF);
            bytes[1] = (byte)(r2 >> 8);
            bytes[0] = (byte)(r2 & 0xFF);

            return BitConverter.ToSingle(bytes, 0);
        }

        private void EnsureLen(ushort[] regs, int len)
        {
            if (regs.Length < len) throw new ArgumentException("Register uzunluğu yetersiz.");
        }
    }
}
