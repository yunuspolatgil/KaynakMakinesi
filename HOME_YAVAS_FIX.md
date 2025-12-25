# ?? HOME YAVAÞ YAZMA HATASI - ÇÖZÜM

## SORUN
```
[22:31:46] Home parametreleri ayarlanýyor (Hýz: 1000, Yavaþ: 500)...
[22:31:46] ? HATA: Home yavaþ deðeri yazýlamadý!
```

## ?? GERÇEK NEDEN

**Tag veritabanýnda YOK!**

K0_Home_Yavas tag'i **tag.json** dosyasýnda var AMA **SQLite veritabanýna** import edilmemiþ!

### Kanýt:
```
Tag.json'da:
- K0_Home_Hiz: 42013 (Int32) ?
- K0_Home_Yavas: 42015 (Int32) ?

ANCAK:
- Veritabanýna import edilmemiþ ?
- TagService tag'i bulamýyor ?
- WriteTagAsync false dönüyor ?
```

---

## ? ÇÖZÜM (3 ADIM)

### ADIM 1: Tag Manager'ý Aç
```
Ana Form ? Tag Yönetimi
```

### ADIM 2: tag.json'ý Ýçe Aktar
```
1. Tag Manager ? "Ýçe Aktar" butonu
2. tag.json dosyasýný seç (Y:\KaynakMakinesi\KaynakMakinesi.UI\tag.json)
3. Import tamamlanýnca "KAYDET" butonuna BAS
```

### ADIM 3: UYGULAMAYI YENÝDEN BAÞLAT
```
?? ÖNEMLÝ: Uygulamayý KAPAT ve YENÝDEN AÇ!

Neden?
- AddressResolver tag cache'i baþlangýçta yüklenir
- TagService tag tanýmlarýný baþlangýçta okur
- Yeni tag'ler için RESTART þart!
```

---

## ?? KONTROL

### Tag Manager'da Kontrol Et:
```
1. Tag Manager aç
2. "K0_Home" diye ara
3. Þunlarý görmelisin:
   - K0_Home_Hiz (42013, Int32)
   - K0_Home_Yavas (42015, Int32)
   - K0_Home_Git (Bool)
   - K0_Home_Switch (10002, Bool)
```

### Test Et:
```
1. Tag Manager'da K0_Home_Yavas'ý seç
2. "Seç + Yaz" butonuna bas
3. 500 yaz
4. Baþarýlý olmalý ?
```

Eðer Tag Manager'da YAZMA BAÞARILI oluyorsa:
? Kalibrasyon formunda da çalýþýr
? Sorun çözülmüþtür!

---

## ?? ALTERNAT ÝF ÇÖZÜM (Manuel Tag Ekleme)

Eðer import çalýþmazsa manuel ekle:

```
Tag Manager ? "Yeni Tag"

Tag 1:
- Name: K0_Home_Hiz
- Address: 42013
- Type: Int32
- Group: Motor_K0
- Description: Home hýz (mm/min)
- PollMs: 250

Tag 2:
- Name: K0_Home_Yavas
- Address: 42015
- Type: Int32
- Group: Motor_K0
- Description: Home yavaþ (mm/min)
- PollMs: 250

KAYDET ? UYGULAMAYI YENÝDEN BAÞLAT!
```

---

## ?? DEBUG BÝLGÝSÝ

Eðer hala çalýþmazsa Log'larda þunlarý ara:

**BAÞARISIZ:**
```
[Debug] TagService - Tag tanýmýný bul: K0_Home_Yavas
[Warn]  TagService - Yazýlmaya çalýþýlan tag tanýmlanmamýþ: K0_Home_Yavas
```

**BAÞARILI:**
```
[Debug] TagService - Tag yazýlýyor: K0_Home_Yavas -> Adres: 42015 = 500
[Debug] ModbusService - WriteAuto: 42015 = 500
[Debug] TagService - Tag yazýldý: K0_Home_Yavas = 500
```

---

## ?? ÖZET ÇÖZÜM

| Adým | Aksiyon | Durum |
|------|---------|-------|
| 1 | Tag Manager Aç | ? |
| 2 | tag.json Import Et | ? |
| 3 | KAYDET Bas | ? |
| 4 | Uygulamayý KAPAT | ? |
| 5 | Uygulamayý AÇ | ? |
| 6 | Tag Manager'da Test Et | ? |
| 7 | Kalibrasyon Formunda Dene | ? |

---

## ?? HIZLI TEST

```
1. UYGULAMAYI KAPAT
2. Tag Manager aç
3. tag.json import et
4. KAYDET
5. UYGULAMAYI AÇ
6. K0 Kalibrasyon formu aç
7. Home iþlemi dene
8. ? "Home yavaþ deðeri yazýldý" göreceksin!
```

---

**SON GÜNCELLEME:** 2025-01-25 22:35  
**SORUN:** Tag veritabanýnda yok  
**ÇÖZÜM:** tag.json import et + RESTART
