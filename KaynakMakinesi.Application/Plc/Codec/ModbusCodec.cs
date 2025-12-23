using System;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Plc.Codec;
using ValueType = KaynakMakinesi.Core.Plc.ValueType;

namespace KaynakMakinesi.Application.Plc.Codec
{
    public sealed class ModbusCodec : IModbusCodec
    {
        // Konfigürasyon A:
        // 32-bit değerlerde word swap AÇIK, word içi byte sırası KAPALI.
        // Çoğu PLC'de float/int için yaygın dizilim: [loWord][hiWord].
        public bool SwapWordsFor32Bit { get; set; } = true;

        // Word içi byte sırasını değiştirmiyoruz.
        public bool SwapBytesInWord { get; set; } = true;

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
                    throw new NotSupportedException("Bool decode burada yapılmaz (coil/di).");
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

            if (SwapWordsFor32Bit) { var t = r0; r0 = r1; r1 = t; }

            // r0 = high word, r1 = low word
            int hi = r0 << 16;
            int lo = r1;
            return hi | lo;
        }

        private float ToFloat(ushort r0, ushort r1)
        {
            r0 = NormalizeWord(r0);
            r1 = NormalizeWord(r1);

            if (SwapWordsFor32Bit) { var t = r0; r0 = r1; r1 = t; }

            // 32-bit int üzerinden float'a
            var bytes = new byte[4];
            bytes[0] = (byte)(r1 & 0xFF);
            bytes[1] = (byte)(r1 >> 8);
            bytes[2] = (byte)(r0 & 0xFF);
            bytes[3] = (byte)(r0 >> 8);

            return BitConverter.ToSingle(bytes, 0);
        }

        private ushort[] FromInt32(int v)
        {
            // hi word / lo word
            ushort hi = (ushort)((v >> 16) & 0xFFFF);
            ushort lo = (ushort)(v & 0xFFFF);

            hi = NormalizeWord(hi);
            lo = NormalizeWord(lo);

            return SwapWordsFor32Bit ? new[] { lo, hi } : new[] { hi, lo };
        }

        private ushort[] FromFloat(float f)
        {
            var bytes = BitConverter.GetBytes(f);

            // bytes little-endian => word’lere çevir
            ushort lo = (ushort)(bytes[0] | (bytes[1] << 8));
            ushort hi = (ushort)(bytes[2] | (bytes[3] << 8));

            hi = NormalizeWord(hi);
            lo = NormalizeWord(lo);

            return SwapWordsFor32Bit ? new[] { lo, hi } : new[] { hi, lo };
        }

        private void EnsureLen(ushort[] regs, int len)
        {
            if (regs.Length < len) throw new ArgumentException("Register uzunluğu yetersiz.");
        }
    }
}
