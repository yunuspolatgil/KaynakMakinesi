# ?? MODBUS ADRESLEME HATASI DÜZELTMESÝ

## ?? TESPÝT EDÝLEN SORUNLAR

### 1. **Discrete Input (IP) Adresleme Hatasý**

**Sorun:**
```csharp
// YANLIÞ
case "IP":
    address1Based = 10000 + idx;  // IP0 -> 10000 ?
```

IP0, 10000 yerine **10001** olmalýydý! Bu yüzden:
- CPU_IP0 (Home Üst Switch) = 10001 (doðru)
- Ama kod IP0'ý 10000 olarak yorumluyordu

**Sonuç:**
- IP0 yerine IP1 okunuyordu
- Home switch sensörü yanlýþ adres

---

### 2. **Holding Register (MW) Adresleme Hatasý**

**Sorun:**
```csharp
// YANLIÞ
case "MW":
    address1Based = 40000 + idx;  // MW0 -> 40000 ?
```

MW0, 40000 yerine **40001** olmalýydý!

**Sonuç:**
- MW0 yazma iþlemi baþarýsýz oluyordu
- "Home yavaþ deðeri yazýlamadý!" hatasý

---

## ? YAPILAN DÜZELTÝLER

### 1. **TryResolveByOperand - Operand Adresleme**

```csharp
// ? DÜZELTÝLDÝ
switch (prefix)
{
    case "MW":
        address1Based = 40001 + idx;  // MW0 -> 40001
        break;
    case "MI":
        address1Based = 42001 + (idx * 2);  // MI0 -> 42001, MI1 -> 42003
        break;
    case "IP":
        address1Based = 10001 + idx;  // IP0 -> 10001, IP1 -> 10002
        break;
    case "IW":
        address1Based = 30001 + idx;  // IW0 -> 30001
        break;
    case "MB":
        address1Based = 1 + idx;  // MB0 -> 1
        break;
}
```

---

### 2. **GetHumanBase1Based - Base Adresler**

```csharp
// ? DÜZELTÝLDÝ
public int GetHumanBase1Based(ModbusArea area)
{
    switch (area)
    {
        case ModbusArea.Coil: return 1;  // MB0 = 1
        case ModbusArea.DiscreteInput: return 10001;  // IP0 = 10001
        case ModbusArea.InputRegister: return 30001;  // IW0 = 30001
        case ModbusArea.HoldingRegister: return 40001;  // MW0 = 40001
        default: return 0;
    }
}
```

---

### 3. **ProfileRule - Adres Aralýklarý**

```csharp
// ? DÜZELTÝLDÝ
new ProfileRule
{ 
    From1Based = 40001,  // MW0 = 40001
    To1Based = 49999, 
    Area = ModbusArea.HoldingRegister
},
new ProfileRule
{ 
    From1Based = 10001,  // IP0 = 10001
    To1Based = 19999, 
    Area = ModbusArea.DiscreteInput
},
new ProfileRule
{ 
    From1Based = 30001,  // IW0 = 30001
    To1Based = 39999, 
    Area = ModbusArea.InputRegister
},
new ProfileRule
{ 
    From1Based = 1,  // MB0 = 1
    To1Based = 9999, 
    Area = ModbusArea.Coil
}
```

---

## ?? DOÐRU MODBUS ADRESLEME TABLOSU

| GMT PLC Operand | Modbus Adresi | NModbus Start0 | Açýklama |
|-----------------|---------------|----------------|----------|
| **MW0** | 40001 | 0 | Holding Register Word 0 |
| **MW1** | 40002 | 1 | Holding Register Word 1 |
| **MI0** | 42001 | 2000 | Integer (2 word) |
| **MI1** | 42003 | 2002 | Integer (2 word) |
| **MF0** | 42017 | 2016 | Float (2 word) |
| **MB0** | 1 | 0 | Coil 0 |
| **MB1** | 2 | 1 | Coil 1 |
| **IP0** | 10001 | 0 | Discrete Input 0 |
| **IP1** | 10002 | 1 | Discrete Input 1 |
| **IP2** | 10003 | 2 | Discrete Input 2 |
| **IP3** | 10004 | 3 | Discrete Input 3 |
| **IW0** | 30001 | 0 | Input Register 0 |

---

## ?? ÖNEMLÝ NOTLAR

### Modbus 1-Based Adresleme

GMT PLC dokümantasyonuna göre:
- **Tüm Modbus adresleri 1-based**
- Start0 (NModbus offset) = Address1Based - Base

**Örnek:**
```
MW0:
  - Address1Based: 40001
  - Base: 40001
  - Start0: 0

IP1:
  - Address1Based: 10002
  - Base: 10001
  - Start0: 1
```

---

## ?? ÖNCE VE SONRA

### ÖNCE (Hatalý):
```csharp
// IP0 okuma
IP0 ? 10000 ? Start0=9999 ? HATALI!
      (Base 10001 olduðu için negatif offset!)

// MW0 yazma
MW0 ? 40000 ? Start0=65535 ? HATALI!
      (Base 40001 olduðu için negatif offset!)
```

### SONRA (Düzeltilmiþ):
```csharp
// IP0 okuma
IP0 ? 10001 ? Start0=0 ? DOÐRU

// IP1 okuma
IP1 ? 10002 ? Start0=1 ? DOÐRU

// MW0 yazma
MW0 ? 40001 ? Start0=0 ? DOÐRU

// MI0 (42001) yazma
MI0 ? 42001 ? Start0=2000 ? DOÐRU
```

---

## ?? TEST ÖNERÝLERÝ

### 1. Discrete Input Test
```csharp
// Tag Manager'da test et:
CPU_IP0 (10001) ? Home Üst Switch ? Read ? Deðer: false/true
CPU_IP1 (10002) ? Home Switch ? Read ? Deðer: false/true
CPU_IP3 (10004) ? Acil Stop ? Read ? Deðer: false/true
```

### 2. Holding Register Test
```csharp
// MW0 yazma testi
Tag: K0_Home_Yavas
Address: 42015 (MI)
Type: Int32
Yaz: 500 ? Baþarýlý ?
```

### 3. Kalibrasyon Formu Test
```
1. K0 Kalibrasyon formunu aç
2. "Home Git" butonu
3. Home Yavaþ: 500 yaz
4. Home Hýz: 1000 yaz
5. "Home'a Git" ? Baþarýlý olmalý ?
```

---

## ?? ÖZET

| Sorun | Neden | Çözüm | Durum |
|-------|-------|-------|-------|
| IP0 = 10000 | Base yanlýþ | IP0 = 10001 | ? Düzeltildi |
| MW0 = 40000 | Base yanlýþ | MW0 = 40001 | ? Düzeltildi |
| Home yavaþ yazýlamýyor | MW0 adresleme hatasý | MI adresleme düzeltildi | ? Düzeltildi |
| IP1 yerine IP0 okunuyor | Offset -1 | Offset düzeltildi | ? Düzeltildi |

---

## ?? SONRAKÝ ADIMLAR

1. ? **Build baþarýlý**
2. ? **Uygulamayý baþlat**
3. ? **Tag Manager'da test et**:
   - CPU_IP0, CPU_IP1 oku
   - MW0, MI0 yaz
4. ? **K0 Kalibrasyon formunu test et**:
   - Home git iþlemi
   - Home yavaþ yazma
5. ? **PLC ile gerçek test**

---

**Düzeltme Tarihi:** 2025-01-25  
**Versiyon:** 1.2.1  
**Etkilenen Dosyalar:**
- `KaynakMakinesi.Infrastructure\Plc\Profile\Gmt496Profile.cs`
