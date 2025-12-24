# 🔧 KaynakMakinesi - PLC Tabanlı Kaynak Makinesi Kontrol Sistemi

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![C#](https://img.shields.io/badge/C%23-7.3-green.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)
[![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/yunuspolatgil/KaynakMakinesi)

Endüstriyel kaynak makineleri için geliştirilmiş, GMT PLC 496T ile haberleşen profesyonel bir kontrol ve izleme sistemi.

## 📋 İçindekiler

- [Özellikler](#-özellikler)
- [Mimari](#-mimari)
- [Teknolojiler](#-teknolojiler)
- [Kurulum](#-kurulum)
- [Kullanım](#-kullanım)
- [Yapılandırma](#-yapılandırma)
- [PLC Haberleşmesi](#-plc-haberleşmesi)
- [Geliştirme](#-geliştirme)
- [Değişiklikler](#-değişiklikler)
- [Lisans](#-lisans)

---

## ✨ Özellikler

### 🎯 Ana Özellikler

- **Gerçek Zamanlı PLC Haberleşmesi**: Modbus TCP/IP üzerinden GMT PLC 496T ile iletişim
- **Tag Yönetim Sistemi**: Esnek tag tanımlama, gruplandırma ve izleme
- **Otomatik Yeniden Bağlanma**: Bağlantı kopmaları durumunda otomatik recovery
- **Heartbeat Monitoring**: PLC bağlantı sağlığı sürekli kontrol
- **Job Queue Sistemi**: Asenkron işlem kuyruğu ve retry mekanizması
- **Gelişmiş Logging**: Multi-sink log sistemi (Bellek + SQLite)
- **Veri Kodlama/Çözme**: Modbus register encoding/decoding (Float, Int32, UShort, Bool)
- **Kullanıcı Dostu Arayüz**: DevExpress kontrolleri ile modern UI
- **Validasyon Sistemi**: Ayar dosyaları ve kullanıcı girişleri için kapsamlı validasyon
- **Graceful Degradation**: Hata durumlarında sistem çökmeden devam ediyor

### 🔐 Güvenlik ve Güvenilirlik

- ✅ Null-safe kod yapısı (NullLogger pattern)
- ✅ Thread-safe operasyonlar
- ✅ Exception handling her katmanda
- ✅ Atomic dosya yazma operasyonları
- ✅ Settings validation ve fallback
- ✅ PLC communication resilience

---

## 🏗️ Mimari

Proje **Clean Architecture** prensiplerine uygun 4 katmanlı yapıda tasarlanmıştır:

```
KaynakMakinesi/
├── 📦 KaynakMakinesi.Core          # Domain Layer
│   ├── Abstractions                 # Core interfaces
│   ├── Jobs                         # Job models
│   ├── Logging                      # Logging abstractions
│   ├── Model                        # Domain models
│   ├── Plc                          # PLC abstractions
│   ├── Settings                     # Configuration models
│   └── Tags                         # Tag definitions
│
├── 📦 KaynakMakinesi.Application   # Application Layer
│   ├── Jobs                         # Job runner
│   ├── Plc                          # PLC services
│   │   ├── Addressing               # Address resolution
│   │   ├── Codec                    # Modbus encoding/decoding
│   │   └── Service                  # Modbus service
│   └── Tags                         # Tag service
│
├── 📦 KaynakMakinesi.Infrastructure # Infrastructure Layer
│   ├── Db                           # SQLite database
│   ├── Jobs                         # Job repository
│   ├── Logging                      # Logger implementations
│   ├── Plc                          # PLC client & profiles
│   │   └── Profile                  # GMT 496T profile
│   ├── Settings                     # Settings store
│   └── Tags                         # Tag repository
│
└── 📦 KaynakMakinesi.UI            # Presentation Layer
    ├── Controls                     # Custom controls
    ├── Forms                        # Application forms
    └── Utils                        # UI helpers
```

### Katman Sorumlulukları

| Katman | Sorumluluk | Bağımlılık |
|--------|------------|------------|
| **Core** | Domain modelleri, interface'ler | Hiçbiri |
| **Application** | İş mantığı, use case'ler | Core |
| **Infrastructure** | Dış sistemler, database, I/O | Core, Application |
| **UI** | Kullanıcı arayüzü, presentation logic | Tüm katmanlar |

---

## 🛠️ Teknolojiler

### Framework & Platform
- **.NET Framework 4.8**
- **C# 7.3**
- **Windows Forms**

### Kütüphaneler
- **DevExpress WinForms** - Modern UI bileşenleri
- **NModbus** - Modbus TCP/IP haberleşmesi
- **Dapper** - Lightweight ORM
- **System.Data.SQLite** - Embedded database
- **Newtonsoft.Json** - JSON serialization

### Araçlar
- **Visual Studio 2019+**
- **SQLite Database Browser** (Opsiyonel)
- **Git** - Versiyon kontrolü

---

## 📥 Kurulum

### Gereksinimler

- Windows 7 SP1 / Windows 10 / Windows 11
- .NET Framework 4.8 Runtime
- 50 MB boş disk alanı
- Ağ bağlantısı (PLC haberleşmesi için)

### Adımlar

1. **Repository'yi klonlayın:**
```bash
git clone https://github.com/yunuspolatgil/KaynakMakinesi.git
cd KaynakMakinesi
```

2. **Bağımlılıkları restore edin:**
```bash
# Visual Studio'da solution'ı açın ve:
# Tools -> NuGet Package Manager -> Restore NuGet Packages
```

3. **Projeyi derleyin:**
```bash
# Visual Studio'da F6 veya Build -> Build Solution
```

4. **Uygulamayı çalıştırın:**
```bash
# F5 ile Debug modda çalıştırın
# veya
cd KaynakMakinesi.UI\bin\Release
KaynakMakinesi.UI.exe
```

---

## 🚀 Kullanım

### İlk Başlatma

Uygulama ilk çalıştırıldığında:
1. `%AppData%\KaynakMakinesi` klasörü otomatik oluşturulur
2. Default ayar dosyası (`appsettings.json`) oluşturulur
3. SQLite veritabanı (`app.db`) initialize edilir

### PLC Bağlantısı

1. **Ayarlar** menüsünden PLC ayarlarını yapılandırın:
   - IP Adresi: `192.168.0.10` (örnek)
   - Port: `502`
   - Unit ID: `1`
   - Timeout: `1500 ms`

2. **Test Connection** butonuyla bağlantıyı test edin

3. Ayarları **kaydedin** - uygulama otomatik olarak yeniden bağlanır

### Tag Yönetimi

#### Yeni Tag Ekleme

1. Tag Yönetimi formunu açın
2. "Yeni Tag" butonuna tıklayın
3. Bilgileri doldurun:
   - **Ad**: MyTag
   - **Adres**: MW100 (veya 40101)
   - **Tip**: Float / Int32 / UShort / Bool
   - **Grup**: Sistem1
   - **Açıklama**: Açıklama metni
   - **Poll Interval**: 250 ms
4. Kaydet

#### Excel/CSV'den Tag İçe Aktarma

Örnek CSV formatı:
```csv
Name,Address,Type,Group,Description,PollMs,ReadOnly
MotorSpeed,MW0,Float,Motor,Motor hızı,250,0
Temperature,MI5,Int32,Sensor,Sıcaklık sensörü,500,1
SetPoint,MW10,UShort,Control,Set point değeri,1000,0
```

1. CSV dosyasını hazırlayın
2. **İçe Aktar** butonuna tıklayın
3. Dosyayı seçin ve onaylayın

---

## ⚙️ Yapılandırma

### appsettings.json

```json
{
  "Plc": {
    "Ip": "192.168.0.10",
    "Port": 502,
    "UnitId": 1,
    "TimeoutMs": 1500,
    "HeartbeatAddress": 0,
    "HeartbeatIntervalMs": 750
  },
  "Database": {
    "FileName": "app.db"
  },
  "Logging": {
    "MinLevel": "Info",
    "KeepInMemory": 2000
  }
}
```

### Validation Kuralları

| Parametre | Min | Max | Default | Açıklama |
|-----------|-----|-----|---------|----------|
| Port | 1 | 65535 | 502 | Modbus TCP portu |
| UnitId | 1 | 247 | 1 | Modbus Slave ID |
| TimeoutMs | 100 | 30000 | 1500 | İletişim timeout |
| HeartbeatIntervalMs | 100 | 60000 | 750 | Bağlantı kontrol aralığı |
| KeepInMemory | 100 | 100000 | 2000 | Bellekte tutulacak log sayısı |

---

## 📡 PLC Haberleşmesi

### GMT PLC 496T Operand Mapping

| Operand | Modbus Adresi | Tip | Açıklama |
|---------|---------------|-----|----------|
| **MW0-MW9999** | 40001-50000 | UShort | Holding Registers (Word) |
| **MI0-MI11** | 42001-42023 | Int32 | Integer Registers (2 word) |
| **MF0-MF4** | 42017, 42019, 42025, 42027, 42029 | Float | Real Registers (2 word) |
| **MB0-MB9999** | 1-10000 | Bool | Coils (Bit) |
| **IP0-IP9999** | 10001-20000 | Bool | Discrete Inputs (Bit) |
| **IW0-IW9999** | 30001-40000 | UShort | Input Registers (Word) |

### Modbus Codec Ayarları

```csharp
// GMT 496T için önerilen ayarlar:
SwapWordsFor32Bit = true   // CDAB format (32-bit değerler)
SwapBytesInWord = false    // Normal byte order
```

### Örnek Kullanım

```csharp
// Tag üzerinden okuma
var result = await tagService.ReadTagAsync("MotorSpeed");
if (result.Success)
    Console.WriteLine($"Motor Speed: {result.Value}");

// Tag üzerinden yazma
await tagService.WriteTagTextAsync("SetPoint", "123.45");

// Doğrudan adres üzerinden
var modbusResult = await modbusService.ReadAutoAsync("MW100");
await modbusService.WriteAutoAsync("MW100", 42.5f);
```

---

## 👨‍💻 Geliştirme

### Proje Yapısı

```bash
# Yeni feature branch oluşturma
git checkout -b feature/yeni-ozellik

# Değişiklikleri commit etme
git add .
git commit -m "feat: yeni özellik eklendi"

# Push etme
git push origin feature/yeni-ozellik
```

### Kod Standartları

- **Naming Convention**: PascalCase (classes), camelCase (private fields)
- **Async/Await**: `ConfigureAwait(false)` kullanımı (UI thread dışında)
- **Exception Handling**: Her public method try-catch ile korunmalı
- **Logging**: Önemli işlemler loglanmalı
- **Null Safety**: Her zaman null kontrolleri yapılmalı

---

## 📝 Değişiklikler

### v1.1.0 (2025-01-XX) - Kritik İyileştirmeler

#### 🎉 Yeni Özellikler
- ✅ **NullLogger Pattern**: Null reference riski ortadan kaldırıldı
- ✅ **AppSettings Validation**: Kapsamlı ayar doğrulama sistemi
- ✅ **Trace/Debug/Fatal Metodları**: Tüm log seviyeleri için convenience metodlar
- ✅ **Constants Kullanımı**: Magic number'lar temizlendi

#### 🐛 Düzeltmeler
- ✅ **Klasör Adı Düzeltmesi**: `Jops` → `Jobs`
- ✅ **Null Safety İyileştirmeleri**: AppLogger ve tüm servislerde
- ✅ **Settings Validation**: Yükleme sırasında otomatik doğrulama
- ✅ **Graceful Fallback**: Geçersiz ayarlarda default'a dönüş

#### 📖 Dokümantasyon
- ✅ **README.md**: Kapsamlı proje dokümantasyonu
- ✅ **KRITIK_SORUN_DUZELTMELERI.md**: Detaylı değişiklik raporu
- ✅ **Kod İçi Yorumlar**: XML documentation başlatıldı

#### 🔧 Teknik İyileştirmeler
- ✅ **JsonFileSettingsStore**: Atomic write + validation
- ✅ **Program.cs**: Try-catch ile korunan başlatma
- ✅ **IAppLogger Interface**: Genişletilmiş metodlar

### v1.0.0 - İlk Sürüm
- 🎯 Temel PLC haberleşmesi (Modbus TCP)
- 🎯 Tag yönetim sistemi
- 🎯 Job queue implementasyonu
- 🎯 DevExpress UI entegrasyonu
- 🎯 SQLite database entegrasyonu
- 🎯 Otomatik reconnect mekanizması
- 🎯 Multi-sink logging sistemi

Detaylı değişiklik listesi için: [KRITIK_SORUN_DUZELTMELERI.md](KRITIK_SORUN_DUZELTMELERI.md)

---

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

---

## 🙏 Teşekkürler

- **DevExpress** - Modern UI bileşenleri için
- **NModbus** - Modbus haberleşme kütüphanesi için
- **SQLite** - Lightweight database için
- **Dapper** - Micro ORM için

---

## 📞 İletişim

Proje Sahibi: **Yunus Polat**

- GitHub: [@yunuspolatgil](https://github.com/yunuspolatgil)
- Repository: [KaynakMakinesi](https://github.com/yunuspolatgil/KaynakMakinesi)

---

## 🔗 Faydalı Bağlantılar

- [.NET Framework Documentation](https://docs.microsoft.com/en-us/dotnet/framework/)
- [Modbus Protocol Specification](https://modbus.org/specs.php)
- [DevExpress WinForms Documentation](https://docs.devexpress.com/WindowsForms/2162/winforms)
- [SQLite Documentation](https://www.sqlite.org/docs.html)

---

<div align="center">

**⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın! ⭐**

Made with ❤️ by Yunus Polat

</div>
