namespace KaynakMakinesi.Core.Plc
{
    public enum ModbusArea
    {
        Coil,
        DiscreteInput,
        HoldingRegister,
        InputRegister
    }

    public enum ValueType
    {
        Bool,
        UShort,
        Int32,
        Float
    }

    public sealed class ResolvedAddress
    {
        public ModbusArea Area { get; set; }
        public ushort Start0 { get; set; }     // 0-based offset (NModbus için)
        public ushort Length { get; set; }     // register sayısı (word). Bool için 1
        public ValueType Type { get; set; }
        public bool ReadOnly { get; set; }

        public int HumanAddress1Based { get; set; } // 42001 gibi (log/debug için)
    }
}