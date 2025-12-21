using System;
using System.Collections.Generic;
using KaynakMakinesi.Core.Plc.Addressing;
using KaynakMakinesi.Core.Plc.Profile;
using KaynakMakinesi.Core.Tags;

namespace KaynakMakinesi.Application.Plc.Addressing
{
    public sealed class AddressResolver : IAddressResolver
    {
        private readonly IPlcProfile _profile;
        private readonly ITagRepository _tags;

        // Tag cache: Name -> Tag
        private volatile Dictionary<string, TagDef> _byName =
            new Dictionary<string, TagDef>(StringComparer.OrdinalIgnoreCase);

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
                var dict = new Dictionary<string, TagDef>(StringComparer.OrdinalIgnoreCase);

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
                _byName = new Dictionary<string, TagDef>(StringComparer.OrdinalIgnoreCase);
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
                // TagDef içinde Address1Based var diye varsayıyorum (senin eski kod böyle kullanıyor)
                if (_profile.TryResolveByModbusAddress(tag.Address1Based, out var addr, out var err))
                {
                    addr.ReadOnly = addr.ReadOnly || tag.ReadOnly; // birleşik
                    res.Success = true;
                    res.Address = addr;
                    res.NormalizedInput = tag.Name;
                    return res;
                }

                res.Error = "Tag adresi profile uymuyor: " + err;
                return res;
            }

            // 2) Sayı mı? (00002 gibi de olur)
            if (int.TryParse(input, out var address1Based))
            {
                if (_profile.TryResolveByModbusAddress(address1Based, out var addr, out var err))
                {
                    res.Success = true;
                    res.Address = addr;
                    res.NormalizedInput = address1Based.ToString();
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
