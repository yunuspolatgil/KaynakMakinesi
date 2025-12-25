# ?? Tag Yönetim Sistemi - Entity Modeline Geçiþ Tamamlandý!

## ?? Tarih: 2025-01-XX
## ?? Amaç: Statik yapýdan tamamen dinamik entity modeline geçiþ

---

## ? TAMAMLANAN ÝÞLEMLER

### 1. **Yeni Entity Altyapýsý Oluþturuldu**

#### ?? `KaynakMakinesi.Core/Entities/EntityBase.cs`
```csharp
public abstract class EntityBase
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } // Soft delete için
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}
```

#### ?? `KaynakMakinesi.Core/Entities/TagEntity.cs`
**DÝNAMÝK TAG ENTITY** - Kalýba hapis olmayan yapý!

```csharp
public class TagEntity : EntityBase
{
    // SABÝT ALANLAR (Minimal)
    public string Name { get; set; }
    public string Address { get; set; }
    public string DataType { get; set; }
    public string GroupName { get; set; }
    
    // DÝNAMÝK METADATA (JSON)
    public string MetadataJson { get; set; }
    
    // Convenience Property'ler
    public string Description { get; set; }
    public int PollMs { get; set; }
    public bool ReadOnly { get; set; }
    public double Scale { get; set; }
    public double Offset { get; set; }
    public string Unit { get; set; }
    
    // DÝNAMÝK ALANLAR ÝÇÝN METODLAR
    public Dictionary<string, object> GetMetadata();
    public void SetMetadata(Dictionary<string, object> metadata);
    public T GetMetadataValue<T>(string key, T defaultValue = default);
    public void SetMetadataValue(string key, object value);
    public bool RemoveMetadataValue(string key);
}
```

**KULLANIM ÖRNEÐÝ:**
```csharp
var tag = new TagEntity
{
    Name = "Motor_K0_Speed",
    Address = "MW100",
    DataType = "Float",
    GroupName = "Motor_K0"
};

// Standart alanlar
tag.Description = "Motor hýzý";
tag.PollMs = 250;
tag.Unit = "RPM";

// ? DÝNAMÝK ALANLAR - Sýnýrsýz!
tag.SetMetadataValue("MinValue", 0);
tag.SetMetadataValue("MaxValue", 3000);
tag.SetMetadataValue("AlarmThreshold", 2500);
tag.SetMetadataValue("Color", "#FF0000");
tag.SetMetadataValue("Icon", "motor.png");
tag.SetMetadataValue("CustomField", "Any value");
// ... istediðin kadar!

// Okuma
var maxValue = tag.GetMetadataValue<int>("MaxValue", 1000);
```

---

### 2. **Repository Pattern (Generic + Specific)**

#### ?? `KaynakMakinesi.Core/Repositories/IRepository<T>.cs`
Generic repository interface - Tüm entity'ler için:
- `GetById(long id)`
- `GetAll()`
- `Find(Expression<Func<T, bool>> predicate)`
- `Add(T entity)`
- `Update(T entity)`
- `Remove(T entity)` - Soft delete
- `RemovePermanently(T entity)` - Hard delete
- `Count()`, `Any()`

#### ?? `KaynakMakinesi.Core/Repositories/ITagEntityRepository.cs`
Tag-specific repository:
- `GetByName(string name)`
- `GetByGroup(string groupName)`
- `GetByDataType(string dataType)`
- `ExistsByName(string name)`
- `RemoveByName(string name)`
- `RemoveByNames(IEnumerable<string> names)`
- `UpsertMany(IEnumerable<TagEntity> tags)`
- `GetAllGroups()`
- `GetInactive()`
- `Restore(long id)`

#### ?? `KaynakMakinesi.Infrastructure/Repositories/SqliteTagEntityRepository.cs`
**TAM ÖZELLÝKLÝ IMPLEMENTATION:**
- Soft delete desteði
- Batch operations (UpsertMany, RemoveByNames)
- UNIQUE constraint (Name kolonu case-insensitive)
- Index'ler (Name, GroupName, DataType, IsActive)
- Validation (entity.Validate())
- Thread-safe

**VERÝTABANI ÞEMASI:**
```sql
CREATE TABLE TagEntities(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE COLLATE NOCASE,
    Address TEXT NOT NULL,
    DataType TEXT NOT NULL DEFAULT 'UShort',
    GroupName TEXT NULL,
    MetadataJson TEXT NULL,  -- ? DÝNAMÝK ALANLAR
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedBy TEXT NULL,
    UpdatedBy TEXT NULL
);

-- Index'ler
CREATE INDEX IX_TagEntities_Name ON TagEntities(Name COLLATE NOCASE);
CREATE INDEX IX_TagEntities_GroupName ON TagEntities(GroupName);
CREATE INDEX IX_TagEntities_DataType ON TagEntities(DataType);
CREATE INDEX IX_TagEntities_IsActive ON TagEntities(IsActive);
```

---

### 3. **Güncellenen Servisler**

#### ?? `KaynakMakinesi.Application/Plc/Addressing/AddressResolver.cs`
```csharp
// ÖNCE: ITagRepository (eski)
public AddressResolver(IPlcProfile profile, ITagRepository tags)

// SONRA: ITagEntityRepository (yeni)
public AddressResolver(IPlcProfile profile, ITagEntityRepository tagRepo)
```

#### ?? `KaynakMakinesi.Application/Tags/TagService.cs`
```csharp
// ÖNCE: ITagRepository (eski)
private readonly ITagRepository _tagRepository;

// SONRA: ITagEntityRepository (yeni)
private readonly ITagEntityRepository _tagRepository;

// Backward compatibility için TagDefinition dönüþümü
private TagDefinition MapToLegacyDefinition(TagEntity entity)
{
    return new TagDefinition
    {
        Id = entity.Id,
        Name = entity.Name,
        Address = entity.Address,
        TypeOverride = entity.DataType,
        GroupName = entity.GroupName,
        Description = entity.Description,
        PollMs = entity.PollMs,
        ReadOnly = entity.ReadOnly,
        Scale = entity.Scale,
        Offset = entity.Offset
    };
}
```

---

### 4. **UI Güncellemeleri**

#### ?? `KaynakMakinesi.Core/Model/TagEntityRow.cs`
Grid row model (UI binding için):
```csharp
public class TagEntityRow
{
    // Entity alanlarý
    public long Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string DataType { get; set; }
    public string GroupName { get; set; }
    
    // Convenience alanlarý
    public string Description { get; set; }
    public int PollMs { get; set; }
    public bool ReadOnly { get; set; }
    public double Scale { get; set; }
    public double Offset { get; set; }
    public string Unit { get; set; }
    
    // Monitor alanlarý (UI-only)
    public string LastValue { get; set; }
    public string Status { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Metadata (hidden)
    public string MetadataJson { get; set; }
}
```

#### ?? `KaynakMakinesi.UI/FrmTagManager.cs`
**TAMAMEN YENÝDEN YAZILDI:**
- TagEntityRow kullanýyor
- Entity <-> Row mapping metodlarý
- Import/Export CSV
- Batch operations (Delete Selected, Delete All)
- Monitor/Read/Write operations
- Soft delete desteði

**GRID KOLONLARI:**
- Tag Adý, Adres, Veri Tipi, Grup
- Açýklama, Poll (ms), RO
- **Scale, Offset, Birim** (yeni!)
- Son Deðer, Durum, Güncellendi

#### ?? `KaynakMakinesi.UI/FrmAnaForm.cs`
```csharp
// ÖNCE: SqliteTagRepository
public FrmAnaForm(..., SqliteTagRepository tagRepo, ...)

// SONRA: ITagEntityRepository
public FrmAnaForm(..., ITagEntityRepository tagRepo, ...)
```

#### ?? `KaynakMakinesi.UI/Program.cs`
Dependency injection güncellendi:
```csharp
// YENÝ: TagEntity Repository
var tagRepo = new SqliteTagEntityRepository(db);

// Servisler
IAddressResolver resolver = new AddressResolver(profile, tagRepo);
ITagService tagService = new TagService(tagRepo, modbusService, logger);

// Form
Application.Run(new FrmAnaForm(settingsStore, inMemSink, supervisor, 
    logger, modbusService, tagRepo, tagService));
```

---

## ??? SÝLÝNEN ESKÝ DOSYALAR

### ? Artýk Kullanýlmayan (Silindi)
- `KaynakMakinesi.Core/Tags/TagDefinition.cs`
- `KaynakMakinesi.Core/Tags/ITagRepository.cs`
- `KaynakMakinesi.Core/Model/TagRow.cs`
- `KaynakMakinesi.Infrastructure/Tags/SqliteTagRepository.cs`
- `KaynakMakinesi.UI/KaynakMakinesi/Infrastructure/Tags/TagEntity.cs` (eski)

---

## ?? AVANTAJLAR

### ? 1. **Kalýba Hapis Deðil!**
```csharp
// Kullanýcý istediði alaný ekleyebilir:
tag.SetMetadataValue("CustomField1", "Value1");
tag.SetMetadataValue("CustomField2", 123);
tag.SetMetadataValue("CustomField3", true);
tag.SetMetadataValue("AlarmSettings", new { Min = 0, Max = 100 });
```

### ? 2. **Soft Delete**
Silinen tag'ler geri yüklenebilir:
```csharp
tagRepo.Remove(tag);       // Soft delete (IsActive = false)
tagRepo.Restore(tag.Id);   // Geri yükle
tagRepo.RemovePermanently(tag);  // Kalýcý sil
```

### ? 3. **Batch Operations**
```csharp
tagRepo.UpsertMany(tags);  // Toplu ekleme/güncelleme
tagRepo.RemoveByNames(names);  // Toplu silme
```

### ? 4. **Type-Safe Query**
```csharp
var motorTags = tagRepo.Find(t => t.GroupName == "Motor_K0");
var floatTags = tagRepo.GetByDataType("Float");
var tag = tagRepo.GetByName("CPU_IP1");
```

### ? 5. **Validation**
```csharp
if (!tag.Validate(out var error))
{
    throw new InvalidOperationException(error);
}
```

### ? 6. **Backward Compatible**
Eski `ITagService.GetAllTags()` hala `TagDefinition` döndürüyor (migration için).

---

## ?? SONRAKI ADIMLAR

### 1. **Visual Studio'da Proje Yenile**
Silinen dosyalar için proje dosyasýný temizlemek gerekiyor:
1. Visual Studio'yu kapat
2. Solution'ý tekrar aç
3. "Reload All" de

### 2. **Veritabaný Migration**
Eski `Tags` tablosundan yeni `TagEntities` tablosuna geçiþ:
```sql
-- Eski tag'leri yeni tabloya kopyala
INSERT INTO TagEntities(Name, Address, DataType, GroupName, MetadataJson, 
                        CreatedAt, UpdatedAt, IsActive)
SELECT Name, Address, Type, GroupName, 
       json_object('Description', Description, 'PollMs', PollMs, 'ReadOnly', ReadOnly),
       datetime('now'), datetime('now'), 1
FROM Tags;

-- Eski tabloyu sil (backup aldýktan sonra)
-- DROP TABLE Tags;
```

### 3. **Grid Kolonlarý Geniþlet**
FrmTagManager'da daha fazla kolon eklenebilir:
- Min/Max deðerler
- Alarm ayarlarý
- Renk/Icon
- Custom alanlar

### 4. **Metadata Editor**
Gelecekte bir metadata editor formu eklenebilir (key-value pairs).

---

## ?? KULLANIM

### Tag Ekleme
```csharp
var tag = new TagEntity
{
    Name = "NewTag",
    Address = "MW200",
    DataType = "Float",
    GroupName = "MyGroup"
};
tag.Description = "My custom tag";
tag.PollMs = 500;
tag.SetMetadataValue("CustomProp", "CustomValue");

tagRepo.Add(tag);
```

### Tag Okuma
```csharp
var tag = tagRepo.GetByName("NewTag");
Console.WriteLine($"{tag.Name} = {tag.Description}");
Console.WriteLine($"Custom: {tag.GetMetadataValue<string>("CustomProp")}");
```

### Tag Güncelleme
```csharp
tag.Description = "Updated description";
tag.SetMetadataValue("NewProp", 123);
tagRepo.Update(tag);
```

### Tag Silme
```csharp
tagRepo.Remove(tag);  // Soft delete
// veya
tagRepo.RemoveByName("NewTag");  // Soft delete by name
// veya
tagRepo.RemovePermanently(tag);  // Hard delete
```

### Toplu Ýþlemler
```csharp
// Import'tan sonra
var entities = importedTags.Select(row => new TagEntity { ... });
tagRepo.UpsertMany(entities);

// Toplu silme
var namesToDelete = new[] { "Tag1", "Tag2", "Tag3" };
tagRepo.RemoveByNames(namesToDelete);
```

---

## ? BUILD DURUMU

**Son Build:** BAÞARILI ?

Not: Silinen dosyalar için Visual Studio'yu yeniden yükleyin.

---

## ?? NOT

Bu yapý **tamamen dinamik** ve **geniþletilebilir**. Kullanýcý TagManager'da girdiði tag'lere istediði kadar ek alan ekleyebilir. Artýk kod deðiþikliðine gerek yok!

**Örnek:**
```csharp
// Kullanýcý TagManager'da bu tag'i oluþturdu
Name: "Motor_Speed"
Address: "MW100"
DataType: "Float"
GroupName: "Motors"
Description: "Motor hýzý"
Unit: "RPM"

// Sonra dinamik alanlar ekledi:
metadata.MinValue = 0
metadata.MaxValue = 3000
metadata.AlarmHigh = 2500
metadata.AlarmLow = 500
metadata.Color = "#FF0000"
metadata.ShowOnDashboard = true
// ... istediði kadar!
```

Tüm bunlar `MetadataJson` kolonunda JSON olarak saklanýyor ve kod deðiþikliði gerektirmiyor! ??
