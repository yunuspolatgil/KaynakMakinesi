using System;
using System.Collections.Generic;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Plc.Addressing;
using KaynakMakinesi.Core.Plc.Profile;
using KaynakMakinesi.Core.Tags;

namespace KaynakMakinesi.Application.Plc.Addressing
{
    public sealed class AddressResolver : IAddressResolver
    {
        private readonly IPlcProfile _profile;
        private readonly ITagRepository _tags;

        // Tag cache: Name -> TagDefinition
        private volatile Dictionary<string, TagDefinition> _byName =
            new Dictionary<string, TagDefinition>(StringComparer.OrdinalIgnoreCase);

        public AddressResolver(IPlcProfile profile, ITagRepository tags)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _tags = tags ?? throw new ArgumentNullException(nameof(tags));

            ReloadTags(); // ilk yükleme
        }

        /// <summary>
        /// DB'den tag listesini yeniden yükler (Tag Manager kaydetten sonra çağır).
        /// </summary>
        public void ReloadTags()
        {
            try
            {
                // ITagRepository içinde ListAll() olmalı
                var list = _tags.ListAll();
                var dict = new Dictionary<string, TagDefinition>(StringComparer.OrdinalIgnoreCase);

                if (list != null)
                {
                    foreach (var t in list)
                    {
                        if (t == null) continue;
                        var name = (t.Name ?? "").Trim();
                        if (name.Length == 0) continue;
                        dict[name] = t;
                    }
                }

                _byName = dict;
            }
            catch
            {
                // Resolver patlamasın
                _byName = new Dictionary<string, TagDefinition>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public ResolveResult Resolve(string input)
        {
            var res = new ResolveResult { Success = false };

            if (string.IsNullOrWhiteSpace(input))
            {
                res.Error = "Girdi boş.";
                return res;
            }

            input = input.Trim();

            // 1) Tag adı mı?
            var dict = _byName;
            if (dict.TryGetValue(input, out var tag))
            {
                // ÖNEMLİ DÜZELTİLDİ: Address field'ını kullan (Address1Based yerine)
                // Address boş değilse onu kullan, yoksa Address1Based'i fallback olarak kullan
                string addressToResolve = !string.IsNullOrWhiteSpace(tag.Address)
                    ? tag.Address
                    : tag.Address1Based.ToString();

                // Address bir operand mı (MW0) yoksa sayı mı (42029)?
                ResolvedAddress addr;
                string err;

                // Önce operand olarak dene
                if (_profile.TryResolveByOperand(addressToResolve, out addr, out err))
                {
                    addr.ReadOnly = addr.ReadOnly || tag.ReadOnly;
                    res.Success = true;
                    res.Address = addr;
                    res.NormalizedInput = tag.Name;
                    return res;
                }

                // Operand değilse sayı olarak dene
                if (int.TryParse(addressToResolve, out var address1Based))
                {
                    if (_profile.TryResolveByModbusAddress(address1Based, out addr, out err))
                    {
                        addr.ReadOnly = addr.ReadOnly || tag.ReadOnly;
                        res.Success = true;
                        res.Address = addr;
                        res.NormalizedInput = tag.Name;
                        return res;
                    }
                }

                res.Error = $"Tag adresi çözümlenemedi: {addressToResolve} - {err}";
                return res;
            }

            // 2) Sayı mı? (00002 gibi de olur)
            if (int.TryParse(input, out var address1Based2))
            {
                if (_profile.TryResolveByModbusAddress(address1Based2, out var addr, out var err))
                {
                    res.Success = true;
                    res.Address = addr;
                    res.NormalizedInput = address1Based2.ToString();
                    return res;
                }

                res.Error = err;
                return res;
            }

            // 3) Operand mı? (MW10, MI5...)
            if (_profile.TryResolveByOperand(input, out var opAddr, out var opErr))
            {
                res.Success = true;
                res.Address = opAddr;
                res.NormalizedInput = input.ToUpperInvariant();
                return res;
            }

            res.Error = "Çözümlenemedi (Tag/Adres/Operand değil): " + opErr;
            return res;
        }
    }
}
