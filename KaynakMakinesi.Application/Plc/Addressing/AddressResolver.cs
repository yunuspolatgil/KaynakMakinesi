using System;
using System.Collections.Generic;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Plc.Addressing;
using KaynakMakinesi.Core.Plc.Profile;
using KaynakMakinesi.Core.Entities;
using KaynakMakinesi.Core.Repositories;

namespace KaynakMakinesi.Application.Plc.Addressing
{
    public sealed class AddressResolver : IAddressResolver
    {
        private readonly IPlcProfile _profile;
        private readonly ITagEntityRepository _tagRepo;

        // Tag cache: Name -> TagEntity
        private volatile Dictionary<string, TagEntity> _byName =
            new Dictionary<string, TagEntity>(StringComparer.OrdinalIgnoreCase);

        public AddressResolver(IPlcProfile profile, ITagEntityRepository tagRepo)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _tagRepo = tagRepo ?? throw new ArgumentNullException(nameof(tagRepo));

            ReloadTags(); // ilk yükleme
        }

        /// <summary>
        /// DB'den tag listesini yeniden yükler (Tag Manager kaydetten sonra çağır).
        /// </summary>
        public void ReloadTags()
        {
            try
            {
                var list = _tagRepo.GetAll();
                var dict = new Dictionary<string, TagEntity>(StringComparer.OrdinalIgnoreCase);

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
                _byName = new Dictionary<string, TagEntity>(StringComparer.OrdinalIgnoreCase);
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

            // 1) Tag adı mı kontrol et
            var dict = _byName;
            if (dict.TryGetValue(input, out var tag))
            {
                // Tag bulundu - Address field'ını kullan
                if (string.IsNullOrWhiteSpace(tag.Address))
                {
                    res.Error = $"Tag '{tag.Name}' için Address tanımlanmamış!";
                    return res;
                }

                // Address'i çözümle (operand veya sayısal adres olabilir)
                return ResolveAddress(tag.Address, tag.Name, tag.ReadOnly);
            }

            // 2) Tag değilse direkt adres/operand olarak çözümle
            return ResolveAddress(input, input, false);
        }

        /// <summary>
        /// Address string'ini ResolvedAddress'e çevirir
        /// </summary>
        private ResolveResult ResolveAddress(string addressStr, string normalizedInput, bool tagReadOnly)
        {
            var res = new ResolveResult { Success = false };

            // Önce operand olarak dene (MW0, IP1, MB5, vs)
            if (_profile.TryResolveByOperand(addressStr, out var addr, out var err))
            {
                addr.ReadOnly = addr.ReadOnly || tagReadOnly;
                res.Success = true;
                res.Address = addr;
                res.NormalizedInput = normalizedInput;
                return res;
            }

            // Operand değilse sayısal adres olarak dene (10002, 42019, vs)
            if (int.TryParse(addressStr, out var address1Based))
            {
                if (_profile.TryResolveByModbusAddress(address1Based, out addr, out err))
                {
                    addr.ReadOnly = addr.ReadOnly || tagReadOnly;
                    res.Success = true;
                    res.Address = addr;
                    res.NormalizedInput = normalizedInput;
                    return res;
                }
            }

            res.Error = $"Adres çözümlenemedi: '{addressStr}' - {err}";
            return res;
        }
    }
}
