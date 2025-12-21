namespace KaynakMakinesi.Core.Plc.Addressing
{
    public interface IAddressResolver
    {
        ResolveResult Resolve(string input);
    }
}