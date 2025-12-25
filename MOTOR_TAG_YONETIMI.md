# Motor Tag Yönetimi - Adres Stratejisi

## ?? Nasýl Çalýþýr?

Motor tag'leri **iki katmanlý** bir sistemle yönetilir:

### 1. **Template (Ýlk Oluþturma)**
`MotorTagTemplate.cs` ? Default adresler ile tag tanýmlarý

### 2. **SQLite (Gerçek Veriler)**
`SqliteTagRepository` ? TagManager'dan yönetilen gerçek adresler

---

## ?? Adres Yönetimi Akýþý

```
???????????????????????????????????????????????
? 1. Form Açýlýyor (FrmMotorKalibrasyon)     ?
???????????????????????????????????????????????
               ?
               ?
???????????????????????????????????????????????
? 2. Tag'ler SQLite'da var mý?                ?
???????????????????????????????????????????????
               ?
       ??????????????????
       ?                ?
       ? EVET           ? HAYIR
???????????????   ????????????????????????
? SQLite'dan  ?   ? Template'den Oluþtur ?
? Kullan      ?   ? DefaultAddress ile   ?
? ? BÝTTÝ    ?   ? ? SQLite'a Kaydet    ?
???????????????   ????????????????????????
                              ?
                              ?
                  ????????????????????????????
                  ? Kullanýcý TagManager'dan ?
                  ? Adresleri Düzenler       ?
                  ? ? BÝTTÝ                 ?
                  ????????????????????????????
```

---

## ?? Kullaným Senaryolarý

### **Senaryo 1: Ýlk Kurulum (Yeni Proje)**

```csharp
// 1. Motor form ilk açýlýyor
var config = MotorConfig.Presets.K0_TorcSag;
var form = new FrmMotorKalibrasyon(config, ...);

// 2. Form initialization
// Tag'ler yok ? Template'den oluþturuluyor
// DefaultAddress'ler kullanýlýyor

// 3. TagManager'ý aç
// "Motor_K0" grubunda tag'leri gör
// Adresleri PLC'ye göre düzenle
```

### **Senaryo 2: Ýkinci Açýlýþ (Tag'ler Var)**

```csharp
// 1. Motor form açýlýyor
var form = new FrmMotorKalibrasyon(config, ...);

// 2. Form initialization
// Tag'ler VAR ? SQLite'dan yükleniyor
// Template KULLANILMIYOR! ?
```

### **Senaryo 3: Adres Deðiþikliði**

```
1. TagManager'ý aç
2. "Motor_K0" grubunu filtrele
3. Ýstediðin tag'i seç
4. Address kolonunu düzenle
5. Kaydet
6. Motor formu açýldýðýnda yeni adres kullanýlýr ?
```

### **Senaryo 4: Toplu Adres Güncelleme**

```
1. TagManager ? Export Excel
2. CSV'yi düzenle (adresleri güncelle)
3. TagManager ? Import Excel
4. Tüm adresler güncellenir ?
```

---

## ?? Örnek CSV (Tag Import için)

```csv
Name,Address,Type,Group,Description,PollMs,ReadOnly
K0_Ileri,2,Bool,Motor_K0,Ýleri hareket komutu,250,0
K0_Geri,3,Bool,Motor_K0,Geri hareket komutu,250,0
K0_Home_Git,4,Bool,Motor_K0,Home komutu,250,0
K0_Ileri_Hiz,42001,Int32,Motor_K0,Ýleri hýz (mm/min),250,0
K0_Ileri_Rampa_Hiz,42003,Int32,Motor_K0,Ýleri rampa hýz,250,0
K0_Home_Hiz,42013,Int32,Motor_K0,Home hýz,250,0
K0_Olculen_Pozisyon,42017,Float,Motor_K0,Ölçülen pozisyon,250,0
K0_Cikis_Pozisyonu,42019,Float,Motor_K0,Çýkýþ pozisyonu,250,1
K0_Motor_Hazir,10001,Bool,Motor_K0,Motor hazýr,100,1
K0_Mevcut_Pozisyon,42023,Float,Motor_K0,Anlýk pozisyon,100,1
```

---

## ? Avantajlar

| Özellik | Açýklama |
|---------|----------|
| **Merkezi Yönetim** | Tüm adresler TagManager'da |
| **Kolay Güncelleme** | Import/Export ile toplu deðiþiklik |
| **Otomatik Kurulum** | Ýlk açýlýþta tag'ler otomatik oluþur |
| **Esneklik** | Template deðiþtirmeden adres deðiþimi |
| **Versiyon Kontrolü** | SQLite database backup ile adresler korunur |

---

## ?? ÖNEMLÝ NOTLAR

### ? YANLIÞ Kullaným
```csharp
// MotorTagTemplate.cs dosyasýný düzenleme!
DefaultAddress = "NEW_ADDRESS"  // ? YAPMA!
```

### ? DOÐRU Kullaným
```
TagManager ? Motor_K0 grubunu seç ? Address kolonunu düzenle ? Kaydet
```

---

## ?? Default Adresler Ne Zaman Kullanýlýr?

**SADECE** þu durumlarda:
1. ? Tag'ler hiç oluþturulmamýþsa (ilk açýlýþ)
2. ? Yeni motor ekleniyorsa
3. ? Database temizlenip sýfýrdan baþlanýyorsa

**Diðer durumlarda:**
- ? Template'deki default adresler **KULLANILMAZ**
- ? SQLite'daki adresler **KULLANILIR**

---

## ?? Özet

```
Template (DefaultAddress)  ?  ÝLK OLUÞTURMA
                               ?
                           SQLite
                               ?
                          TagManager  ?  GERÇEK YÖNETÝM
                               ?
                        Motor Formu  ?  KULLANIM
```

**Sonuç:** Adresler **tek yerden** (TagManager) yönetiliyor! ?

---

## ?? Sonraki Adým

Þimdi `FrmMotorKalibrasyon` formunu oluþturabiliriz:
1. ? Tag'leri SQLite'dan kontrol et
2. ? Yoksa template'den oluþtur
3. ? Tag'leri kullanarak PLC haberleþmesi yap

Devam edelim mi? ??
