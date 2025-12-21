
---

## Özellikler

### 1) JSON Ayarları + Ayar Formu
PLC ayarları `%AppData%\KaynakMakinesi\plcsettings.json` dosyasından okunur.  
Dosya değişince `reloadOnChange` ile sistem bunu algılar ve bağlantı servisi yeniden bağlanır.

### 2) Çökmeyen Uygulama
Aşağıdaki global handler’lar ile hatalar yakalanır ve loglanır:
- `Application.ThreadException`
- `AppDomain.CurrentDomain.UnhandledException`
- `TaskScheduler.UnobservedTaskException`

> Uygulama “hata oldu” diye loglar, ama patlayıp kapanmaz.

### 3) Log Sistemi (Dosya + UI)
- **Serilog** ile `logs/log-YYYYMMDD.txt` şeklinde günlük dönen dosya logları
- UI’da canlı izleme için **InMemory LogHub** (son N log)

### 4) Otomatik Reconnect
`PlcConnectionSupervisor`:
- Bağlanır
- Heartbeat ile sağlığı kontrol eder
- Koparsa: `Disconnect → bekle(backoff) → reconnect`
- Ayar değişirse: temiz şekilde yeniden bağlanır

---

## Kurulum

### Gereksinimler
- Visual Studio 2019 / 2022
- .NET Framework 4.8
- (Opsiyonel) DevExpress WinForms

### NuGet Paketleri

**Hosting / Config / DI**
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Configuration.Json`
- `Microsoft.Extensions.Options.ConfigurationExtensions`

**Log**
- `Serilog`
- `Serilog.Sinks.File`
- (Opsiyonel) `Serilog.Extensions.Hosting`

> Not: .NET Framework projelerinde bazen `app.config` içine bindingRedirect eklenmesi gerekebilir. NuGet çoğu zaman bunu otomatik yapar.

---

## PLC Ayarları (JSON)

Dosya yolu:
`%AppData%\KaynakMakinesi\plcsettings.json`

Örnek içerik:

```json
{
  "Plc": {
    "Enabled": true,
    "Protocol": "ModbusTcp",
    "Host": "192.168.1.10",
    "Port": 502,
    "UnitId": 1,
    "ConnectTimeoutMs": 2000,
    "PollIntervalMs": 500,
    "HeartbeatAddress": 0,
    "HeartbeatExpectedValue": 1,
    "Reconnect": { "MinDelayMs": 500, "MaxDelayMs": 10000 }
  }
}
