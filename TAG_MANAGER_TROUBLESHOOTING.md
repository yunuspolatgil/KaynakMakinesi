# ?? TAG MANAGER OKUMA/YAZMA SORUNU - Troubleshooting Kýlavuzu

## ? SORUN TAN IMI
Tag Manager'da "Seç + Oku" ve "Seç + Yaz" butonlarý çalýþmýyor.

## ?? OLASI NEDENLER

### 1. PLC Baðlantýsý Problemi
**Kontrol:**
- Ana form'da sað altta "PLC Durumu" göstergesine bakýn
- Durum: `Connected` olmalý
- Son OK zamaný güncel olmalý

**Çözüm:**
- Ayarlar ? PLC ayarlarýný kontrol edin
- IP: `192.168.0.10` (veya PLC'nizin IP'si)
- Port: `502`
- Test Connection ile baðlantýyý test edin

---

### 2. Tag'ler Veritabanýnda Yok
**Kontrol:**
- Tag Manager'ý açýn
- Grid'de tag'ler görünüyor mu?
- Address kolonunda deðerler var mý?

**Çözüm:**
```
1. Tag Manager ? Ýçe Aktar
2. tag.json dosyasýný seçin
3. Import tamamlandýktan sonra "Kaydet" butonuna basýn
4. Uygulamayý YENDEN BAÞLATIN
```

**Neden yeniden baþlatma?**
- AddressResolver tag'leri uygulama baþlangýcýnda cache'e alýyor
- Yeni tag'ler eklenince cache güncellenmeli

---

### 3. Address Field Boþ
**Kontrol:**
- Tag Manager'da bir tag seçin
- Address kolonuna bakýn
- Deðer var mý? (örn: "10002", "42019", "MW0")

**Çözüm:**
```sql
-- SQLite'da kontrol:
SELECT Name, Address, Type FROM Tags WHERE Address IS NULL OR Address = '';
```

Eðer Address boþsa:
1. Tag Manager'dan manuel düzenleyin
2. VEYA tag.json'ý tekrar import edin

---

### 4. AddressResolver Cache Eski
**Belirti:**
- Tag'ler veritabanýnda var
- PLC baðlý
- Ama yine de okuma/yazma çalýþmýyor

**Çözüm:**
```
UYGULAMAYI YENÝDEN BAÞLATIN!
```

AddressResolver tag cache'i sadece baþlangýçta yüklenir.

---

### 5. ModbusService Null
**Kontrol:**
- Tag Manager açýldýðýnda "Modbus servisi baðlý deðil" uyarýsý alýyor musunuz?

**Çözüm:**
- Program.cs'de ModbusService inject edilmiþ mi kontrol edin
- FrmAnaForm ? btnTagYonetim_ItemClick'e bakýn

---

## ?? TEST ADIMLARI

### Adým 1: PLC Baðlantýsý Testi
```
1. Ana forma dönün
2. Sað altta "PLC Durumu" = Connected mý?
3. Deðilse ? Ayarlar ? Test Connection
```

### Adým 2: Tag Sayýsý Kontrolü
```
1. Tag Manager'ý açýn
2. Kaç adet tag görüyorsunuz?
3. 0 tag ise ? tag.json import edin
```

### Adým 3: Tek Tag Okuma Testi
```
1. Tag Manager'da "CPU_IP1" tag'ini seçin
2. "Seç + Oku" butonuna basýn
3. Status kolonunda ne yazýyor?
   - "Baþarýlý" ? ÇALIÞIYOR ?
   - "Hata: ..." ? Hata mesajýný okuyun
   - Hiç deðiþmedi ? LOG'lara bakýn
```

### Adým 4: Log Kontrolü
```
1. Ana form ? Log paneline bakýn
2. Tag okuma denemesi yapýn
3. Log'da þunlarý arayýn:
   - "Tag okunuyor: CPU_IP1 -> Address: 10002"
   - "? CPU_IP1 = true/false"
   - "? CPU_IP1 okuma hatasý: ..."
```

---

## ?? DEBUG MODU

### Log Seviyesini Debug'a Alýn
```json
// appsettings.json
{
  "Logging": {
    "MinLevel": "Debug",  // ? Info yerine Debug
    "KeepInMemory": 5000
  }
}
```

### Detaylý Log Ýnceleme
Tag okuma/yazma sýrasýnda log'larda þunlarý görmelisiniz:

**Baþarýlý Okuma:**
```
[HH:mm:ss] [Debug] FrmTagManager - Tag okunuyor: K0_Home_Switch -> Address: 10002, Type: Bool
[HH:mm:ss] [Debug] ModbusService - ReadAuto hatasý: 10002
[HH:mm:ss] [Debug] AddressResolver - Resolve: 10002 -> DI 10002
[HH:mm:ss] [Debug] FrmTagManager - ? K0_Home_Switch = false
```

**Baþarýsýz Okuma:**
```
[HH:mm:ss] [Error] FrmTagManager - ? K0_Home_Switch okuma hatasý: ...
```

---

## ? ÇÖZÜM ÖNERÝLERÝ

### Öneri 1: Tag'leri Ýçe Aktarýn
```
1. Tag Manager ? Ýçe Aktar
2. tag.json seçin
3. "Kaydet" ? Uygulama yeniden baþlat
```

### Öneri 2: AddressResolver Cache Yenileme Ekleyin
**Geliþmiþ Çözüm (Kodda Deðiþiklik Gerekir):**

```csharp
// FrmTagManager.cs - Constructor'a IAddressResolver ekle
private readonly IAddressResolver _resolver;

public FrmTagManager(
    SqliteTagRepository tagRepo, 
    IModbusService modbusService, 
    IAppLogger log,
    IAddressResolver resolver)  // ? Ekle
{
    _resolver = resolver;
    // ...
}

// btnSaveChanges_ItemClick içinde:
private void btnSaveChanges_ItemClick(object sender, ItemClickEventArgs e)
{
    SaveToDb();
    
    // Cache'i yenile!
    _resolver?.ReloadTags();
    
    XtraMessageBox.Show("Kaydedildi!", "Tamam");
    LoadFromDb();
}
```

---

## ?? SORUN GÝDERME TABLO

| Belirti | Olasý Neden | Çözüm |
|---------|-------------|--------|
| "Modbus servisi baðlý deðil" | ModbusService null | Uygulamayý yeniden baþlat |
| Status deðiþmiyor | Tag Address boþ | tag.json import et |
| "Hata: Çözümlenemedi" | AddressResolver cache eski | Uygulamayý yeniden baþlat |
| "Hata: PLC okumasý baþarýsýz" | PLC baðlantýsý yok | PLC baðlantýsýný kontrol et |
| Timeout | PLC IP/Port yanlýþ | Ayarlarý kontrol et |

---

## ?? HIZLI ÇÖZÜM

**90% SORUN ÇÖZÜMÜ:**
```
1. Uygulamayý kapat
2. Tag Manager ? tag.json import et (yoksa)
3. Kaydet
4. Uygulamayý baþlat
5. Ana form ? PLC Durumu = Connected olmalý
6. Tag Manager aç
7. Tek bir tag seç (CPU_IP1)
8. "Seç + Oku" ? Baþarýlý olmalý
```

---

## ?? DESTEK

Eðer yukarýdaki adýmlar iþe yaramadýysa:

1. **Log dosyasýný kontrol edin:**
   - `%AppData%\KaynakMakinesi\app.db` ? Logs tablosu

2. **Tag veritabanýný kontrol edin:**
   - SQLite Browser ile app.db'yi açýn
   - Tags tablosuna bakýn
   - Address kolonu dolu mu?

3. **PLC'yi ping atarak test edin:**
   ```cmd
   ping 192.168.0.10
   ```

---

**Son Güncelleme:** 2025-01-25
**Versiyon:** 1.2.0
