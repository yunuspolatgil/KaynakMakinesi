using System;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Plc.Codec;
using ValueType = KaynakMakinesi.Core.Plc.ValueType;

namespace KaynakMakinesi.Application.Plc.Codec
{
    /// <summary>
    /// Modbus codec - GMT 496T PLC için yapılandırılmış
    /// SwapWordsFor32Bit = true (CDAB format)
    /// </summary>
    public sealed class ModbusCodec : IModbusCodec
    {
        public bool SwapWordsFor32Bit { get; set; } = false;
        public bool SwapBytesInWord { get; set; } = false;

        public object Decode(ValueType type, ushort[] regs)
        {       
            if (regs == null || regs.Length == 0)
                throw new ArgumentException("Register boş.");

            switch (type)
            {
                case ValueType.UShort:
                    return NormalizeWord(regs[0]);
                case ValueType.Int32:
                    EnsureLen(regs, 2);
                    return ToInt32(regs[0], regs[1]);
                case ValueType.Float:
                    EnsureLen(regs, 2);
                    return ToFloat(regs[0], regs[1]);
                default:
                    throw new NotSupportedException("Bool decode burada yapılmaz.");
            }
        }

        public ushort[] Encode(ValueType type, object value)
        {
            switch (type)
            {
                case ValueType.UShort:
                    return new[] { NormalizeWord(Convert.ToUInt16(value)) };
                case ValueType.Int32:
                    return FromInt32(Convert.ToInt32(value));
                case ValueType.Float:
                    return FromFloat(Convert.ToSingle(value));
                default:
                    throw new NotSupportedException("Bool encode coil ile yazılır.");
            }
        }

        private ushort NormalizeWord(ushort w)
        {
            if (!SwapBytesInWord) return w;
            return (ushort)((w >> 8) | (w << 8));
        }

        private int ToInt32(ushort r0, ushort r1)
        {
            r0 = NormalizeWord(r0);
            r1 = NormalizeWord(r1);

            if (SwapWordsFor32Bit)
            {
                var temp = r0;
                r0 = r1;
                r1 = temp;
            }

            return (r0 << 16) | r1;
        }

        private float ToFloat(ushort r0, ushort r1)
        {
            r0 = NormalizeWord(r0);
            r1 = NormalizeWord(r1);

            if (SwapWordsFor32Bit)
            {
                var temp = r0;
                r0 = r1;
                r1 = temp;
            }

            var bytes = new byte[4];
            bytes[0] = (byte)(r1 & 0xFF);
            bytes[1] = (byte)((r1 >> 8) & 0xFF);
            bytes[2] = (byte)(r0 & 0xFF);
            bytes[3] = (byte)((r0 >> 8) & 0xFF);

            return BitConverter.ToSingle(bytes, 0);
        }

        private ushort[] FromInt32(int v)
        {
            ushort hi = (ushort)((v >> 16) & 0xFFFF);
            ushort lo = (ushort)(v & 0xFFFF);

            hi = NormalizeWord(hi);
            lo = NormalizeWord(lo);

            return SwapWordsFor32Bit ? new[] { lo, hi } : new[] { hi, lo };
        }

        private ushort[] FromFloat(float f)
        {
            var bytes = BitConverter.GetBytes(f);

            ushort lo = (ushort)(bytes[0] | (bytes[1] << 8));
            ushort hi = (ushort)(bytes[2] | (bytes[3] << 8));

            lo = NormalizeWord(lo);
            hi = NormalizeWord(hi);

            return SwapWordsFor32Bit ? new[] { lo, hi } : new[] { hi, lo };
        }

        private void EnsureLen(ushort[] regs, int len)
        {
            if (regs.Length < len) 
                throw new ArgumentException($"Register uzunluğu yetersiz. Beklenen: {len}, Gelen: {regs.Length}");
        }
    }
}
