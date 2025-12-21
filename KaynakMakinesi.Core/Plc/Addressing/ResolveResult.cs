namespace KaynakMakinesi.Core.Plc.Addressing
{
    public sealed class ResolveResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public Plc.ResolvedAddress Address { get; set; }
        public string NormalizedInput { get; set; }
    }
}
