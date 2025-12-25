# Modbus Coil Adresleme Sorunu - 0-Based/1-Based Düzeltmesi

## ?? Sorun Açıklaması

**Belirtilen Sorun:**
> Bit değerler için sorun var. Örneğin PLC'de adı "K0_ileri" olan tag'in adresi "00001". TagManager'da bu adrese TRUE yazdığımda "00002" tetikleniyor.

### Kök Neden Analizi

Modbus protokolünde **Coil** (bit) adresleri için **0-based** ve **1-based** karışıklığı vardı.

#### Yanlış Davranış:
```
PLC'de görünen: Coil 1 (00001)
Database'deki adres: 1
Hesaplanan Start0: 1 - 0 = 1  ? YANLIŞ
NModbus'a gönderilen: WriteSingleCoil(unitId, start0=1, ...)
Tetiklenen coil: Coil 2  ? SORUN!
```

#### Doğru Davranış Olmalı:
```
PLC'de görünen: Coil 1 (00001)
Database'deki adres: 1
Hesaplanan Start0: 1 - 1 = 0  ? DOĞRU
NModbus'a gönderilen: WriteSingleCoil(unitId, start0=0, ...)
Tetiklenen coil: Coil 1  ? DOĞRU!
```

---

## ?? Yapılan Düzeltmeler

### 1. **Gmt496Profile.cs** - Coil Base Address Değiştirildi

#### Değişiklik 1: ProfileRule - From1Based

**ESKI:**
```csharp
new ProfileRule
{ 
    From1Based = 0,  // YANLIŞ - Coil 0 yoktur!
    To1Based = 9999, 
    Area = ModbusArea.Coil, 
    ...
}
```

**YENİ:**
```csharp
new ProfileRule
{ 
    From1Based = 1,  // ÖNEMLİ: Coil adresleri 1-based (1-9999)
    To1Based = 9999, 
    Area = ModbusArea.Coil, 
    ...
}
```

#### Değişiklik 2: GetHumanBase1Based

**ESKI:**
```csharp
public int GetHumanBase1Based(ModbusArea area)
{
    switch (area)
    {
        case ModbusArea.Coil: return 0;  // YANLIŞ!
        ...
    }
}
```

**YENİ:**
```csharp
public int GetHumanBase1Based(ModbusArea area)
{
    switch (area)
    {
        case ModbusArea.Coil: return 1;  // Coil adresleri 1'den başlar (1-based)
        case ModbusArea.DiscreteInput: return 10000;
        case ModbusArea.InputRegister: return 30000;
        case ModbusArea.HoldingRegister: return 40000;
        ...
    }
}
```

#### Değişiklik 3: TryResolveByOperand - MB ve QP

**ESKI:**
```csharp
case "MB":
    address1Based = 0 + idx;  // MB0 -> 0  ? YANLIŞ
    break;
case "QP":
    address1Based = 2000 + idx;  // QP0 -> 2000  ? YANLIŞ
    break;
```

**YENİ:**
```csharp
case "MB":
    // MB0 -> Coil 1 (çünkü Coil adresleri 1-based)
    // MB1 -> Coil 2
    address1Based = 1 + idx;  // MB0 -> 1, MB1 -> 2  ? DOĞRU
    break;
case "QP":
    // QP (Output Coils) de 1-based
    address1Based = 1 + idx;  // QP0 -> 1, QP1 -> 2  ? DOĞRU
    break;
```

---

### 2. **FrmTagManager.cs** - Import Sırasında Coil Adreslerinin Düzeltilmesi

**ESKI:**
```csharp
switch (typeCode)
{
    case 0: // Coil (MB) - base = 0
        baseAddress = 0;
        break;
    ...
}

// GMT'de Addr zaten offset içeriyor, direkt topla
int modbusAddr = baseAddress + address;  // 0 + 1 = 1  ? YANLIŞ
```

**YENİ:**
```csharp
switch (typeCode)
{
    case 0: // Coil (MB) - Coil adresleri 1-based
        // Addr=1 -> address1Based=1, Addr=2 -> address1Based=2
        baseAddress = 0;  // Base 0 çünkü Addr zaten 1'den başlıyor
        break;
    ...
}

// Coil için Addr direkt adrestir (1-based)
// Diğerleri için base + offset
int modbusAddr;
if (typeCode == 0)
{
    // Coil: Addr direkt 1-based adrestir
    modbusAddr = address;  // Direkt kullan  ? DOĞRU
}
else
{
    // Diğerleri: Base + Offset
    modbusAddr = baseAddress + address;
}
```

---

## ?? Modbus Adres Mapping Tablosu (Güncellenmiş)

### Coil Adresleri (Bit - Read/Write)

| PLC'de Görünen | GMT Addr Kolonu | Modbus 1-Based | Start0 (NModbus) | Açıklama |
|----------------|-----------------|----------------|------------------|----------|
| Coil 1 | 1 | 1 | 0 | İlk coil |
| Coil 2 | 2 | 2 | 1 | İkinci coil |
| Coil 3 | 3 | 3 | 2 | Üçüncü coil |
| ... | ... | ... | ... | ... |
| Coil 9999 | 9999 | 9999 | 9998 | Son coil |

**Formül:**
```csharp
// Human 1-based ? NModbus 0-based
Start0 = address1Based - 1
```

**Örnek:**
```csharp
// Test.gpf.csv'den: K0_Ileri, AddrType=0, Addr=2
address1Based = 2  // Direkt Addr değeri (Coil için base yoktur)
Start0 = 2 - 1 = 1  // NModbus'a gönderilecek offset
```

### Diğer Adresler

| Alan | PLC Gösterimi | Addr Kolonu | Modbus 1-Based | Start0 | Base |
|------|---------------|-------------|----------------|--------|------|
| **Holding Register** | MW0-MW9999 | 0-9999 | 40000-49999 | offset | 40000 |
| **Discrete Input** | IP0-IP9999 | 0-9999 | 10000-19999 | offset | 10000 |
| **Input Register** | IW0-IW9999 | 0-9999 | 30000-39999 | offset | 30000 |

---

## ?? Test Senaryoları

### Senaryo 1: Coil Yazma (Test.gpf.csv)

**Test Dosyası:**
```csv
"K0_Ileri"    0   2   ...  # AddrType=0 (Coil), Addr=2
```

**Beklenen Davranış:**
```
1. Import ? Address field: "2"
2. Resolve: address1Based = 2, Start0 = 1
3. Write(unitId, start0=1, value=true)
4. PLC'de Coil 2 tetikleniyor  ?
```

**Test Kodu:**
```csharp
// TagManager'da "K0_Ileri" tag'ini seç
// "Yaz" butonuna bas, değer = True gir
// PLC'de Coil 2'nin tetiklendiğini gör
```

### Senaryo 2: Birden Fazla Coil

| Tag | Addr | Beklenen Start0 | PLC'de Tetiklenen |
|-----|------|-----------------|-------------------|
| K0_Ileri | 2 | 1 | Coil 2 ? |
| K0_Geri | 3 | 2 | Coil 3 ? |
| K0_Home_Git | 4 | 3 | Coil 4 ? |

---

## ?? Adres Hesaplama Algoritması

### TryResolveByModbusAddress

```csharp
public bool TryResolveByModbusAddress(int address1Based, out ResolvedAddress resolved, out string error)
{
    // Rule bulma
    var rule = _rules.FirstOrDefault(r => address1Based >= r.From1Based && address1Based <= r.To1Based);
    
    // Base address al (Coil için 1, diğerleri için standart)
    var baseAddress = GetHumanBase1Based(rule.Area);
    
    // Start0 hesapla (0-based offset)
    ushort start0 = (ushort)(address1Based - baseAddress);
    
    // Örnek: Coil 2
    // address1Based = 2
    // baseAddress = 1
    // start0 = 2 - 1 = 1  ? DOĞRU
    
    resolved = new ResolvedAddress
    {
        Area = rule.Area,
        Start0 = start0,  // NModbus için 0-based offset
        HumanAddress1Based = address1Based  // Log için 1-based
    };
}
```

---

## ? Doğrulama

### Manuel Test Adımları

1. **Import Test:**
   ```
   - Test.gpf.csv dosyasını import edin
   - "K0_Ileri" tag'inin Address değerine bakın
   - Beklenen: "2" olmalı (operand değil!)
   ```

2. **Okuma Testi:**
   ```
   - PLC'de Coil 2'yi manuel olarak aktif edin
   - TagManager'da "K0_Ileri" tag'ini seçin
   - "Oku" butonuna basın
   - Beklenen: true değerini görmelisiniz
   ```

3. **Yazma Testi:**
   ```
   - "K0_Ileri" tag'ini seçin
   - "Yaz" ? true girin
   - PLC'de Coil 2'nin tetiklendiğini kontrol edin
   - Beklenen: Sadece Coil 2 aktif olmalı, Coil 3 değil!
   ```

### Otomatik Test

```csharp
[Test]
public void CoilAddressResolution_Test()
{
    var profile = new Gmt496Profile();
    
    // Test 1: address1Based = 1 ? Start0 = 0
    var result = profile.TryResolveByModbusAddress(1, out var addr, out var error);
    Assert.IsTrue(result);
    Assert.AreEqual(0, addr.Start0);
    
    // Test 2: address1Based = 2 ? Start0 = 1
    result = profile.TryResolveByModbusAddress(2, out addr, out error);
    Assert.IsTrue(result);
    Assert.AreEqual(1, addr.Start0);
    
    // Test 3: MB0 operand ? address1Based = 1 ? Start0 = 0
    result = profile.TryResolveByOperand("MB0", out addr, out error);
    Assert.IsTrue(result);
    Assert.AreEqual(1, addr.HumanAddress1Based);
    Assert.AreEqual(0, addr.Start0);
}
```

---

## ?? Özet

### Değişiklik Tablosu

| Dosya | Metod/Property | Eski Değer | Yeni Değer | Sebep |
|-------|----------------|------------|------------|-------|
| `Gmt496Profile.cs` | `_rules[Coil].From1Based` | 0 | 1 | Coil adresleri 1'den başlar |
| `Gmt496Profile.cs` | `GetHumanBase1Based(Coil)` | 0 | 1 | 1-based adresle 0-based offset |
| `Gmt496Profile.cs` | `TryResolveByOperand("MB0")` | 0 | 1 | MB0 = Coil 1 |
| `Gmt496Profile.cs` | `TryResolveByOperand("QP0")` | 2000 | 1 | QP0 = Coil 1 |
| `FrmTagManager.cs` | `BtnImportExcel` (Coil import) | base+addr | addr | Coil Addr direkt 1-based |

### Etkilenen Alanlar

? **Coil (MB, QP)** - DÜZELTİLDİ
- 1-based adreslemeyegeçildi
- Start0 hesaplaması düzeltildi
- Import mantığı güncellendi

? **Etkilenmedi:**
- Holding Register (MW, MI, MF) - Zaten doğru çalışıyordu
- Discrete Input (IP) - Zaten doğru çalışıyordu
- Input Register (IW) - Zaten doğru çalışıyordu

---

## ?? Build Status

? **Build Successful** - Tüm değişiklikler derlendi

---

## ?? Tarih
**2025-01-XX** - Modbus Coil adresleme sorunu düzeltildi

## ?? Geliştirici
Yunus Polat

---

**NOT:** Bu düzeltme sadece **Coil** (bit) adreslerini etkiler. Holding Register, Input Register ve Discrete Input adresleri değişmedi.
