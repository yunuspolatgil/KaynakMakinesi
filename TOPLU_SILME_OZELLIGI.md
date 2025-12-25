# TagManager Çoklu Seçim ve Toplu Silme Özelliði

## ?? Eklenen Özellikler

### 1. ? Çoklu Seçim (Multi-Select)

Grid'de artýk **checkbox** ile çoklu seçim yapýlabilir.

**Grid Ayarlarý:**
```csharp
gvTags.OptionsSelection.MultiSelect = true;
gvTags.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
gvTags.OptionsSelection.CheckBoxSelectorColumnWidth = 40;
```

**Kullaným:**
- Grid'in sol tarafýnda checkbox kolonu otomatik görünür
- Tek týkla satýr seçimi
- Header checkbox ile tümünü seç/kaldýr
- CTRL+Click veya SHIFT+Click ile de seçim yapýlabilir

---

### 2. ??? Yeni Butonlar

#### **Seçilenleri Sil** (`btnDeleteSelected`)

**Konum:** Ribbon ? Düzenleme grubu

**Ýþlev:**
- Seçili tüm tag'leri siler
- Onay mesajý gösterir
- Ýlk 5 tag'i listeler, fazlasý varsa "... ve X tane daha" gösterir
- Database'den siler ve grid'i günceller

**Kod:**
```csharp
private void BtnDeleteSelected_ItemClick(object sender, ItemClickEventArgs e)
{
    // 1. Seçili satýrlarý al
    var selectedRows = gvTags.GetSelectedRows();
    
    // 2. TagRow nesnelerine dönüþtür
    var tagsToDelete = new List<TagRow>();
    foreach (var rowHandle in selectedRows)
    {
        var row = gvTags.GetRow(rowHandle) as TagRow;
        if (row != null) tagsToDelete.Add(row);
    }
    
    // 3. Onay mesajý
    // Tek tag: "Tag adý silinsin mi?"
    // Çoklu: "X adet tag silinsin mi? Ýlk 5: ..."
    
    // 4. Database'den sil + Grid'den kaldýr
    foreach (var tag in tagsToDelete)
    {
        _tagRepo.DeleteByName(tag.Name);
        _rows.Remove(tag);
    }
    
    // 5. Grid'i yenile
    gvTags.RefreshData();
    SaveSnapshot();
}
```

---

#### **Tümünü Sil** (`btnDeleteAll`)

**Konum:** Ribbon ? Düzenleme grubu

**Ýþlev:**
- **DÝKKAT:** Tüm tag'leri siler (Tehlikeli!)
- Ýki kademeli onay sistemi:
  1. Ýlk onay: "TÜM TAG'LER SÝLÝNECEK - X adet"
  2. Son onay: "Son onay: X adet tag SÝLÝNSÝN MÝ?"
- Geri alýnamaz iþlem uyarýsý
- Tüm tag'leri database'den siler

**Kod:**
```csharp
private void BtnDeleteAll_ItemClick(object sender, ItemClickEventArgs e)
{
    // 1. Ýlk onay - Tehlikeli iþlem uyarýsý
    var confirmMsg = "DÝKKAT! TÜM TAG'LER SÝLÝNECEK!\n" +
                    $"Toplam {_rows.Count} adet tag kalýcý olarak silinecek.\n" +
                    "Bu iþlem GERÝ ALINAMAZ!";
    
    // 2. Çift onay sistemi
    // Ýkinci onay mesajý
    
    // 3. Tüm tag'leri sil
    var allTags = _rows.ToList();
    foreach (var tag in allTags)
    {
        _tagRepo.DeleteByName(tag.Name);
    }
    
    // 4. Grid'i temizle
    _rows.Clear();
    gvTags.RefreshData();
}
```

---

## ?? Ribbon Düzeni

### Düzenleme Grubu (grpCrud)

Butonlar soldan saða:

1. **Yeni Tag** - Yeni tag ekle
2. **Kaydet** - Deðiþiklikleri kaydet
3. **Tag Sil** - Odaklanýlan tek tag'i sil
4. **Seçilenleri Sil** ? YENÝ
5. **Tümünü Sil** ? YENÝ
6. **Geri Al** - Son snapshot'a dön

---

## ?? Kullaným Senaryolarý

### Senaryo 1: Birkaç Tag Silme

1. Grid'de silmek istediðiniz tag'lerin checkbox'larýný iþaretleyin
2. Ribbon'da **"Seçilenleri Sil"** butonuna týklayýn
3. Onay mesajýný kontrol edin:
   ```
   5 adet tag silinsin mi?
   
   Ýlk 5 tag:
   - CPU_IP1
   - CPU_QP0
   - MHI_Hiz_K0
   - MHI_RHiz_K0
   - MHI_RYavas_K0
   ```
4. **Evet** ? Tag'ler silinir
5. Baþarý mesajý: "5 adet tag baþarýyla silindi."

---

### Senaryo 2: Tüm Tag'leri Silme

?? **DÝKKAT:** Bu iþlem GERÝ ALINAMAZ!

1. Ribbon'da **"Tümünü Sil"** butonuna týklayýn
2. Ýlk onay mesajý:
   ```
   DÝKKAT! TÜM TAG'LER SÝLÝNECEK!
   
   Toplam 150 adet tag kalýcý olarak silinecek.
   
   Bu iþlem GERÝ ALINAMAZ!
   
   Devam etmek istediðinize emin misiniz?
   ```
3. **Evet** ? Ýkinci onay mesajý:
   ```
   Son onay: 150 adet tag SÝLÝNSÝN MÝ?
   ```
4. **Evet** ? Tüm tag'ler silinir
5. Baþarý mesajý: "150 adet tag silindi."

---

### Senaryo 3: Header Checkbox ile Tümünü Seç

1. Grid'in en üstündeki checkbox'a týklayýn
2. **Tüm satýrlar seçilir**
3. "Seçilenleri Sil" ile toplu silme yapabilirsiniz

---

## ?? Güvenlik Özellikleri

### 1. Onay Mesajlarý

- **Tek tag:** Basit onay
- **Çoklu tag:** Liste ile detaylý onay
- **Tüm tag'ler:** Çift onay + Uyarý

### 2. Error Handling

```csharp
try
{
    _tagRepo.DeleteByName(tag.Name);
    _rows.Remove(tag);
    deletedCount++;
}
catch (Exception ex)
{
    _log?.Error(nameof(FrmTagManager), $"Tag silme hatasý: {tag.Name}", ex);
    // Hata olsa da diðer tag'leri silmeye devam eder
}
```

### 3. Logging

```csharp
// Seçilenleri sil
_log?.Info(nameof(FrmTagManager), $"{deletedCount} adet tag toplu silindi");

// Tümünü sil
_log?.Warn(nameof(FrmTagManager), $"TÜM TAG'LER SÝLÝNDÝ! Toplam: {deletedCount}");
```

---

## ?? Teknik Detaylar

### Grid Multi-Select Modu

**CheckBoxRowSelect** modu kullanýldý çünkü:
- ? Kullanýcý dostu (görsel checkbox)
- ? Header checkbox ile tümünü seç
- ? Tek týkla seçim/seçim iptali
- ? CTRL/SHIFT tuþlarý da çalýþýr

**Alternatifler:**
- `CellSelect` - Hücre bazlý seçim
- `RowSelect` - CTRL/SHIFT ile satýr seçimi (checkbox yok)

### Event Handler Baðlantýsý

**Designer.cs'de:**
```csharp
this.btnDeleteSelected.ItemClick += 
    new DevExpress.XtraBars.ItemClickEventHandler(this.BtnDeleteSelected_ItemClick);

this.btnDeleteAll.ItemClick += 
    new DevExpress.XtraBars.ItemClickEventHandler(this.BtnDeleteAll_ItemClick);
```

**FrmTagManager.cs'de:**
```csharp
private void BtnDeleteSelected_ItemClick(object sender, ItemClickEventArgs e) { ... }
private void BtnDeleteAll_ItemClick(object sender, ItemClickEventArgs e) { ... }
```

---

## ?? Snapshot Sistemi

Her silme iþleminden sonra snapshot güncellenir:

```csharp
SaveSnapshot();  // Undo için son durum kaydedilir
```

**Geri Al (Undo):**
- Silme iþleminden sonra "Geri Al" butonu kullanýlabilir
- Son snapshot'a geri döner
- ?? Database'e kaydetmeden önce kullanýlmalý!

---

## ?? Önemli Notlar

1. **"Tümünü Sil" butonu tehlikelidir!**
   - Çift onay sistemi var ama yine de dikkatli kullanýlmalý
   - Production ortamýnda bu butonu gizlemeyi düþünün

2. **Silme iþlemi kalýcýdýr:**
   - Database'den silme anýnda gerçekleþir
   - "Geri Al" sadece snapshot'a döner (henüz kaydedilmemiþse)
   - Kaydedildikten sonra geri alýnamaz

3. **Performans:**
   - Toplu silme iþlemi her tag için ayrý DELETE sorgusu çalýþtýrýr
   - Çok sayýda tag için (1000+) yavaþ olabilir
   - Ýyileþtirme: Transaction içinde batch delete

---

## ?? Gelecek Ýyileþtirmeler

### Öneriler:

1. **Batch Delete:**
```csharp
_tagRepo.DeleteMany(tagNames);  // Tek transaction'da toplu silme
```

2. **Undo Buffer:**
```csharp
// Silinen tag'leri geçici olarak sakla
_deletedTags.AddRange(tagsToDelete);
// "Geri Getir" butonu ile geri yükle
```

3. **Filter ile Silme:**
```csharp
// "Belirli gruba ait tüm tag'leri sil"
DeleteByFilter(group: "Motor");
```

4. **Export Önce Sil:**
```csharp
// Silmeden önce otomatik export
ExportToBackup();
DeleteSelected();
```

---

## ?? Tarih
**2025-01-XX** - Çoklu seçim ve toplu silme özellikleri eklendi

## ?? Geliþtirici
Yunus Polat

---

## ? Özet

| Özellik | Durum |
|---------|-------|
| Grid multi-select | ? Aktif |
| Checkbox kolon | ? 40px geniþlik |
| Seçilenleri Sil butonu | ? Eklendi |
| Tümünü Sil butonu | ? Eklendi |
| Onay mesajlarý | ? Tek/Çoklu/Tümü |
| Error handling | ? Try-catch |
| Logging | ? Info/Warn |
| Snapshot update | ? Her silmede |

**Baþarýyla uygulandý!** ??
