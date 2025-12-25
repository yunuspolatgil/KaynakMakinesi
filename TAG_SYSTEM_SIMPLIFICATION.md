# ?? TAG SÝSTEMÝ BASÝTLEÞTÝRME RAPORU

## ?? YAPILAN DEÐÝÞÝKLÝKLER

### ? 1. TagDefinition Basitleþtirildi

**Önceki Durum:**
- `Address` (string)
- `Address1Based` (int)
- Ýki field arasýnda tutarsýzlýk

**Yeni Durum:**
- Sadece `Address` (string) kullanýlýyor
- `Address1Based` tamamen kaldýrýldý
- Tüm adresler string olarak saklanýyor: `"10002"`, `"42019"`, `"MW0"`, `"IP1"`

### ? 2. MotorTagTemplate.cs Silindi

**Neden:**
- Tag'ler artýk hardcode edilmiyor
- Tüm tag tanýmlarý SQLite veritabanýnda
- Tag Manager ile yönetiliyor

**Önceki Kod:**
```csharp
// ? KALDIRILDI
public List<TagDefinition> CreateTags()
{
    var templates = MotorTagTemplate.GetStandardMotorTags();
    // ...hardcode tag oluþturma
}
```

**Yeni Yaklaþým:**
- Tag'ler `tag.json` dosyasýndan import edilir
- SQLite veritabanýnda saklanýr
- Tag Manager ile düzenlenir

### ? 3. AddressResolver Basitleþtirildi

**Önceki Durum:**
```csharp
// ? Karmaþýk - hem Address hem Address1Based
string addressToResolve = !string.IsNullOrWhiteSpace(tag.Address)
    ? tag.Address
    : tag.Address1Based.ToString();
```

**Yeni Durum:**
```csharp
// ? Basit - sadece Address
if (string.IsNullOrWhiteSpace(tag.Address))
{
    res.Error = $"Tag '{tag.Name}' için Address tanýmlanmamýþ!";
    return res;
}
return ResolveAddress(tag.Address, tag.Name, tag.ReadOnly);
```

### ? 4. SqliteTagRepository Güncellendi

**Kaldýrýlanlar:**
- `Address1Based` kolonu (schema'dan kaldýrýlmadý ama kullanýlmýyor)

**Güncellenenler:**
```csharp
// Sadece gerekli kolonlar
cmd.CommandText = @"SELECT Id, Name, Address, Type, GroupName, Description, PollMs, ReadOnly
                    FROM Tags ORDER BY Name;";
```

### ? 5. TagService Ýyileþtirildi

**Eklemeler:**
- Address boþ kontrolü
- Daha detaylý log mesajlarý
- Debug/Warn/Error log seviyeleri

```csharp
if (string.IsNullOrWhiteSpace(tagDef.Address))
{
    _logger?.Error(nameof(TagService), $"'{tagName}' tag'i için Address tanýmlanmamýþ!");
    return false;
}
```

---

## ?? MEVCUT TAG YAPISI (tag.json)

### Örnek Tag Tanýmlarý:

```json
{
  "Id": 100,
  "Name": "K0_Home_Switch",
  "Address": "10002",           // ? Discrete Input 10002 (CPU_IP1)
  "Address1Based": 0,            // ? Artýk kullanýlmýyor
  "Type": "Bool",
  "GroupName": "Motor_K0",
  "Description": "Home switch (CPU_IP1)",
  "PollMs": 50,
  "ReadOnly": 1
}
```

**Önemli Notlar:**
- `Address1Based` kolonu veritabanýnda hala var (geriye uyumluluk için)
- Ama artýk hiçbir kod tarafýndan kullanýlmýyor
- Sadece `Address` (string) kullanýlýyor

---

## ?? KULLANIM ÞEKLÝ

### 1. Tag'leri Veritabanýna Yükleme

**Yöntem 1: Tag Manager - Import**
1. Tag Manager'ý aç
2. "Ýçe Aktar" butonuna týkla
3. `tag.json` dosyasýný seç
4. Tag'ler SQLite'a kaydedilir

**Yöntem 2: Direkt SQL**
```sql
-- Örnek tag ekleme
INSERT INTO Tags (Name, Address, Type, GroupName, Description, PollMs, ReadOnly, UpdatedAt)
VALUES ('K0_Test', '42001', 'Int32', 'Motor_K0', 'Test tag', 250, 0, datetime('now'));
```

### 2. Tag Okuma (Kod)

```csharp
// TagService üzerinden
var result = await _tagService.ReadTagAsync("K0_Home_Switch");
if (result.Success)
{
    bool isActive = (bool)result.Value;
    Console.WriteLine($"Home Switch: {isActive}");
}

// BaseKalibrasyonForm'da
var (success, value) = await ReadTagAsync("Home_Switch");  // Prefix otomatik eklenir: "K0_Home_Switch"
```

### 3. Tag Yazma (Kod)

```csharp
// TagService üzerinden
await _tagService.WriteTagAsync("K0_Ileri_Hiz", 1000);

// BaseKalibrasyonForm'da
await WriteTagAsync("Ileri_Hiz", 1000);  // Prefix otomatik eklenir: "K0_Ileri_Hiz"
```

---

## ? TAG ADRESLERÝ DOÐRULAMA

### K0 Motor Tag'leri (tag.json'dan)

| Tag Adý | Address | Type | Açýklama | Doðru? |
|---------|---------|------|----------|--------|
| `K0_Ileri` | `"1"` | Bool | Coil 1 | ? |
| `K0_Geri` | `"2"` | Bool | Coil 2 | ? |
| `K0_Home_Git` | `"3"` | Bool | Coil 3 | ? |
| `K0_Home_Ust_Switch` | `"10001"` | Bool | DI 10001 (CPU_IP0) | ? |
| `K0_Home_Switch` | `"10002"` | Bool | DI 10002 (CPU_IP1) | ? |
| `K0_Acil_Stop` | `"10004"` | Bool | DI 10004 (CPU_IP3) | ? |
| `K0_Ileri_Hiz` | `"42005"` | Int32 | HR 42005 | ? |
| `K0_Ileri_Rampa_Hiz` | `"42009"` | Int32 | HR 42009 | ? |
| `K0_Ileri_Rampa_Yavas` | `"42007"` | Int32 | HR 42007 | ? |
| `K0_Home_Hiz` | `"42013"` | Int32 | HR 42013 | ? |
| `K0_Home_Yavas` | `"42015"` | Int32 | HR 42015 | ? |
| `K0_Olculen_Pozisyon` | `"42017"` | Float | HR 42017 | ? |
| `K0_Cikis_Pozisyonu` | `"42019"` | Float | HR 42019 | ? |
| `K0_Mevcut_Pozisyon` | `"42019"` | Float | HR 42019 | ? |
| `K0_Pozisyon` | `"42021"` | Float | HR 42021 | ? |
| `K0_Pozisyon_Hiz` | `"42023"` | Int32 | HR 42023 | ? |
| `K0_RampaSet_Hizlanma` | `"42025"` | Float | HR 42025 | ? |
| `K0_RampaSet_Yavaslama` | `"42027"` | Float | HR 42027 | ? |

**? Tüm tag adresleri JSON dosyasýnda doðru tanýmlanmýþ!**

---

## ?? MÝGRASYON ADIMLARI

### Adým 1: Mevcut Veritabanýný Temizle (Opsiyonel)

```sql
-- Sadece Motor_K0 grubu tag'lerini sil
DELETE FROM Tags WHERE GroupName = 'Motor_K0';

-- Veya tüm tag'leri sil
DELETE FROM Tags;
```

### Adým 2: Tag'leri Import Et

1. Uygulamayý baþlat
2. Tag Manager'ý aç
3. `tag.json` dosyasýný import et

### Adým 3: Tag'leri Doðrula

Tag Manager'da:
- Address kolonunu kontrol et
- Type kolonunu kontrol et
- GroupName = "Motor_K0" olan tag'leri filtrele
- Her tag'in Address'i dolu olmalý

---

## ?? ÖNEMLÝ NOTLAR

### 1. Address Format

Tag Address field'ý **string** olduðu için þu formatlarda olabilir:

- **Sayýsal Modbus Adresi**: `"10002"`, `"42019"`
- **Operand**: `"IP1"`, `"MW0"`, `"MF2"`

AddressResolver her iki formatý da destekler.

### 2. Tag Naming Convention

Motor tag'leri için prefix kullanýlýr:

```
{MotorName}_{TagSuffix}

Örnekler:
- K0_Home_Switch
- K1_Ileri_Hiz
- K2_Pozisyon
```

### 3. ReadOnly Tag'ler

Bazý tag'ler `ReadOnly = 1` olarak iþaretlenmiþ:

- `K0_Home_Ust_Switch` (Sensor)
- `K0_Home_Switch` (Sensor)
- `K0_Acil_Stop` (Sensor)
- `K0_Cikis_Pozisyonu` (PLC çýkýþý)
- `K0_Mevcut_Pozisyon` (PLC çýkýþý)

Bu tag'lere yazma denemesi yapýlýrsa TagService hata döner.

---

## ?? SONUÇ

Sistem artýk **çok daha basit**:

1. ? **Tek Veri Kaynaðý**: Sadece SQLite veritabaný
2. ? **Tek Address Field**: Sadece `Address` (string)
3. ? **Hardcode Yok**: Tag tanýmlarý artýk kodda deðil, veritabanýnda
4. ? **Esnek Yönetim**: Tag Manager ile kolay düzenleme
5. ? **Tutarlý Sistem**: Address çözümleme tek bir yerde (AddressResolver)

**Tag sistemi artýk kullanýma hazýr!** ??
