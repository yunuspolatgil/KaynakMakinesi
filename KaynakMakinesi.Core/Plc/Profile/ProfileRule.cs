namespace KaynakMakinesi.Core.Plc.Profile
{
    public sealed class ProfileRule
    {
        public int From1Based { get; set; }     // örn 40001
        public int To1Based { get; set; }       // örn 40512
        public ModbusArea Area { get; set; }
        public ValueType Type { get; set; }
        public ushort Length { get; set; }      // word sayısı (Int32/Float için 2)
        public bool ReadOnly { get; set; }

        // 2-word tiplerde hizalama kontrolü için:
        public int Step { get; set; } = 1;      // MW:1, MI/MF:2
    }
}
