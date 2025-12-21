namespace KaynakMakinesi.Core.Plc.Codec
{
    public interface IModbusCodec
    {
        object Decode(ValueType type, ushort[] regs);
        ushort[] Encode(ValueType type, object value);
    }
}
