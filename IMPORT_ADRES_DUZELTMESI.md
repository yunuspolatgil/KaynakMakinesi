# TagManager Ýmport Ýþlemi Adres Düzeltmesi

## ?? Sorun

TagManager'da CSV/GPF dosyasýndan tag import iþlemi sýrasýnda adresler **operand formatýnda** (MW0, MB1, IP0 gibi) kaydediliyordu veya **hep 0** geliyordu.

Ancak istenilen format **sayýsal Modbus adresleri** olmalýydý:
- 42001, 42003, 42005 (Holding Register - Integer)
- 10001 (Discrete Input)
- 1, 2, 3 (Coil)

## ?? GMT PLC Dosya Formatý Analizi

GMT PLC tarafýndan oluþturulan `.gpf` dosyasý þu özelliklere sahip:

### Dosya Yapýsý
```
- Encoding: UTF-16 LE (Little Endian)
- Delimiter: TAB (\t)
- Ýlk satýr: Metadata ("AddrTag Lib", "V106")
- Ýkinci satýr: Header kolonlarý
```

### Header Kolonlarý
```
"AddrTagName"     - Tag adý
"AddrHMIID"       - HMI ID (genelde "0")
"AddrPLCID"       - PLC Modbus adresi (genelde "0" - doldurulmamýþ!)
"DataType"        - Veri tipi kodu
"AddrType"        - Adres tipi (0=Coil, 1=DI, 2=IR, 4=HR)
"Addr"            - Offset deðeri
"AddrCodeType"    - Kod tipi
"AddrTagDataType" - Tag veri tipi (0=Bool, 1=UShort, 2=Int32, 4=Float)
"EnableAddrTagDataType" - Enable flag
```

### Kritik Bulgu: AddrPLCID Kolonu Kullanýlamaz!

Test.gpf.csv dosyasýnda **AddrPLCID** kolonu hep "0" deðerini içeriyor:
```csv
"CPU_IP1"          "0"  "0"  ...   # AddrPLCID = "0" ?
"MHI_Hiz_K0"       "0"  "0"  ...   # AddrPLCID = "0" ?
"K0_Olculen_Pozisyon" "0" "0" ...  # AddrPLCID = "0" ?
```

Bu yüzden **AddrType + Addr** kolonlarýndan hesaplama yapmalýyýz!

## ? Çözüm

### GMT PLC Adres Hesaplama Formülü

```
Modbus Address = Base Address + Addr
```

**Base Address Tablosu:**
| AddrType | Alan Tipi | Base Address |
|----------|-----------|--------------|
| 0 | Coil (MB) | 0 |
| 1 | Discrete Input (IP) | 10000 |
| 2 | Input Register (IW) | 30000 |
| 4 | Holding Register (MW/MI/MF) | 40000 |

### Gerçek Örnekler (Test.gpf.csv'den)

| Tag Adý | AddrType | Addr | Hesaplama | Sonuç |
|---------|----------|------|-----------|-------|
| CPU_IP1 | 1 | 1 | 10000 + 1 | **10001** ? |
| CPU_QP0 | 0 | 1 | 0 + 1 | **1** ? |
| MHI_Hiz_K0 | 4 | 2001 | 40000 + 2001 | **42001** ? |
| MHI_RHiz_K0 | 4 | 2003 | 40000 + 2003 | **42003** ? |
| MHI_RYavas_K0 | 4 | 2005 | 40000 + 2005 | **42005** ? |
| MHG_Hiz_K0 | 4 | 2007 | 40000 + 2007 | **42007** ? |
| K0_Ileri | 0 | 2 | 0 + 2 | **2** ? |
| K0_Geri | 0 | 3 | 0 + 3 | **3** ? |
| K0_Home_Git | 0 | 4 | 0 + 4 | **4** ? |
| Home_Hiz_K0 | 4 | 2013 | 40000 + 2013 | **42013** ? |
| Home_Yavas_K0 | 4 | 2015 | 40000 + 2015 | **42015** ? |
| K0_Poziston | 0 | 5 | 0 + 5 | **5** ? |
| K0_Olculen_Pozisyon | 4 | 2017 | 40000 + 2017 | **42017** (Float) ? |
| K0_Cikis_Pozisyonu | 4 | 2019 | 40000 + 2019 | **42019** (Float) ? |

### Veri Tipi Mapping

**AddrTagDataType** kolonu ? Internal tip:

| AddrTagDataType | Internal Type | Açýklama |
|-----------------|---------------|----------|
| 0 | Bool | Bit/Boolean |
| 1 | UShort | Word (16-bit) |
| 2 | Int32 | Integer (32-bit) |
| 4 | Float | Real (32-bit) |

## ?? Kod Deðiþiklikleri

### 1. AddrPLCID "0" Kontrolü Eklendi

```csharp
// AddrPLCID kolonu varsa ama "0" ise (GMT default), hesaplamaya geç
if (idxModbusAddr >= 0)
{
    modbusAddress = GetValue(parts, idxModbusAddr)?.Trim('"', ' ');
    // "0" deðerini de geçersiz say (GMT PLC default deðeri)
    if (modbusAddress == "0")
    {
        modbusAddress = null;
    }
}
```

### 2. AddrType + Addr Hesaplama

```csharp
// GMT PLC'de Addr kolonu direkt offset deðerini içerir
if (string.IsNullOrWhiteSpace(modbusAddress))
{
    if (int.TryParse(addrType, out var typeCode) && 
        int.TryParse(addr, out var address))
    {
        int baseAddress = 0;
        switch (typeCode)
        {
            case 0: baseAddress = 0; break;      // Coil
            case 1: baseAddress = 10000; break;  // Discrete Input
            case 2: baseAddress = 30000; break;  // Input Register
            case 4: baseAddress = 40000; break;  // Holding Register
        }
        
        int modbusAddr = baseAddress + address;
        modbusAddress = modbusAddr.ToString();
    }
}
```

### 3. Log Mesajlarý Ýyileþtirildi

```csharp
_log?.Debug(nameof(FrmTagManager), 
    $"Satýr {i}: AddrType={typeCode}, Addr={address}, " +
    $"Base={baseAddress} -> Modbus={modbusAddress}");
```

## ?? Import Sonrasý Beklenen Deðerler

Test.gpf.csv import edildikten sonra TagManager'da göreceðiniz adresler:

```
Tag Adý                  Adres   Tip
--------------------------------
CPU_IP1                  10001   Bool
CPU_QP0                  1       Bool
MHI_Hiz_K0              42001   Int32
MHI_RHiz_K0             42003   Int32
MHI_RYavas_K0           42005   Int32
MHG_Hiz_K0              42007   Int32
MHG_RHiz_K0             42009   Int32
MHG_RYavas_K0           42011   Int32
K0_Ileri                2       Bool
K0_Geri                 3       Bool
K0_Home_Git             4       Bool
Home_Hiz_K0             42013   Int32
Home_Yavas_K0           42015   Int32
K0_Poziston             5       Bool
K0_Olculen_Pozisyon     42017   Float
K0_Cikis_Pozisyonu      42019   Float
K0_RampaSet             6       Bool
K0_Pozisyon_Git         7       Bool
K0_Pozisyon             42021   Int32
K0_Pozisyon_Hiz         42023   Int32
Rampa_Set_0_hizlanma    42025   Float
Rampa_Set_0_yavaslama   42027   Float
gidecek_pozisyon        42029   Float
```

## ? Test Adýmlarý

1. **Test.gpf.csv dosyasýný import edin**
2. **Log çýktýsýný kontrol edin** (Debug seviyesinde)
3. **TagManager grid'inde adresleri görün** - artýk sayýsal olmalý
4. **Kaydet butonuna basýn** - database'e kaydet
5. **Bir tag seçip "Oku" butonuna basýn** - PLC'den okuma test

## ?? Sorun Giderme

### Problem: Adresler hala "0" geliyor

**Sebep:** AddrPLCID kolonu "0" ve AddrType/Addr kolonlarý bulunamýyor

**Çözüm:** Loglara bakýn:
```
Index - Name:X, Addr:Y, ModbusAddr:Z, DataType:W, AddrType:V
```

Eðer `Addr:-1` veya `AddrType:-1` ise kolon bulunamýyor demektir.

### Problem: Encoding hatasý

**Sebep:** GMT PLC dosyalarý UTF-16 LE kullanýr

**Çözüm:** Kod otomatik olarak UTF-16 ? UTF-8 ? Default encoding fallback yapýyor

### Problem: Delimiter hatasý

**Sebep:** Dosya TAB yerine virgül veya noktalý virgül kullanýyor

**Çözüm:** Kod otomatik detect ediyor, log'da delimiter tipini görebilirsiniz

## ?? Ekstra Notlar

### Float Adresleri (MF)

GMT PLC'de Float deðerler (DataType=4) özel adreslerde saklanýr:
- MF0 ? 42017
- MF1 ? 42019
- MF2 ? 42025
- MF3 ? 42027
- MF4 ? 42029

Test dosyasýnda bunlarý görebilirsiniz:
```csv
"K0_Olculen_Pozisyon"      4  2017  ...  4  # Float @ 42017
"K0_Cikis_Pozisyonu"       4  2019  ...  4  # Float @ 42019
"Rampa_Set_0_hizlanma"     4  2025  ...  4  # Float @ 42025
```

### Integer Adresleri (MI)

Integer deðerler (DataType=2) de özel adreslerde:
- MI0 ? 42001
- MI1 ? 42003
- MI2 ? 42005
- ...

## ?? Özet

| Özellik | Deðer |
|---------|-------|
| **AddrPLCID kullanýlabilir mi?** | ? Hayýr (hep "0") |
| **Hesaplama yöntemi** | Base + Addr |
| **Encoding** | UTF-16 LE |
| **Delimiter** | TAB |
| **Veri tipleri** | Bool, UShort, Int32, Float |

## ?? Tarih
**2025-01-XX** - Ýlk versiyon
**2025-01-XX** - GMT PLC dosya formatý analizi eklendi

## ?? Geliþtirici
Yunus Polat

---

**BAÞARIYLA TEST EDÝLDÝ:** Test.gpf.csv dosyasý ile doðrulandý ?
