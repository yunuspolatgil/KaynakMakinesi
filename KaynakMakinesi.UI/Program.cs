using KaynakMakinesi.Application.Jobs;
using KaynakMakinesi.Application.Plc.Addressing;
using KaynakMakinesi.Application.Plc.Codec;
using KaynakMakinesi.Application.Plc.Service;
using KaynakMakinesi.Application.Tags;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc.Addressing;
using KaynakMakinesi.Core.Plc.Codec;
using KaynakMakinesi.Core.Plc.Profile;
using KaynakMakinesi.Core.Settings;
using KaynakMakinesi.Core.Tags;
using KaynakMakinesi.Infrastructure.Db;
using KaynakMakinesi.Infrastructure.Jobs;
using KaynakMakinesi.Infrastructure.Logging;
using KaynakMakinesi.Infrastructure.Plc;
using KaynakMakinesi.Infrastructure.Plc.Profile;
using KaynakMakinesi.Infrastructure.Settings;
using KaynakMakinesi.Infrastructure.Tags;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaynakMakinesi.UI
{
    static class Program
    {
        private static IAppLogger _logger = NullLogger.Instance; // Default olarak NullLogger

        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            System.Windows.Forms.Application.ThreadException += (s, e) =>
            {
                _logger.Error("UI", "ThreadException", e.Exception);
                MessageBox.Show(e.Exception.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                _logger.Error("APP", "UnhandledException", ex);
                MessageBox.Show("Kritik hata oluştu.", "Kritik", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                _logger.Error("TASK", "UnobservedTaskException", e.Exception);
                e.SetObserved();
            };

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KaynakMakinesi");
                Directory.CreateDirectory(appFolder);

                var settingsPath = Path.Combine(appFolder, "appsettings.json");
                var settingsStore = new JsonFileSettingsStore(settingsPath);
                var settings = settingsStore.Load();

                // Settings validation (Load içinde yapılıyor ama double-check)
                try
                {
                    settings.Validate();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show($"Ayar dosyası geçersiz:\n{ex.Message}\n\nDefault ayarlar kullanılacak.", 
                        "Ayar Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    settings = new AppSettings();
                    settingsStore.Save(settings);
                }

                var db = new SqliteDb(appFolder, settings.Database.FileName);

                var tagRepo = new SqliteTagRepository(db);
                tagRepo.EnsureSchema();

                IPlcProfile profile = new Gmt496Profile();

                var inMemSink = new InMemoryLogSink(settings.Logging.KeepInMemory);
                var sqliteSink = new SqliteLogSink(db);
                
                // Logger'ı oluştur ve global değişkene ata
                IAppLogger logger = new AppLogger(inMemSink, sqliteSink);
                _logger = logger ?? NullLogger.Instance; // Null safety

                var plcClient = new ModbusPlcClient(logger);
                var supervisor = new PlcConnectionSupervisor(settingsStore, plcClient, logger);

                var jobRepo = new SqliteJobRepository(db);
                var runner = new JobRunner(jobRepo, supervisor, plcClient, logger);

                IAddressResolver resolver = new AddressResolver(profile, tagRepo);
                
                var codec = new ModbusCodec
                {
                    SwapWordsFor32Bit = true,
                    SwapBytesInWord = false
                };
                IModbusCodec iCodec = codec;

                var modbusService = new ModbusService(plcClient, resolver, codec, settingsStore, logger);

                ITagService tagService = new TagService(tagRepo, modbusService, logger);

                logger.Info("Program", $"Uygulama başlatıldı. Database: {db.DbPath}");

                supervisor.Start();
                runner.Start();

                System.Windows.Forms.Application.ApplicationExit += (s, e) =>
                {
                    try { runner.Stop(); } catch { }
                    try { supervisor.Stop(); } catch { }
                    try { plcClient.DisconnectAsync(CancellationToken.None).Wait(500); } catch { }
                    try { plcClient.Dispose(); } catch { }
                    
                    logger.Info("Program", "Uygulama kapatıldı.");
                };

                System.Windows.Forms.Application.Run(new FrmAnaForm(settingsStore, inMemSink, supervisor, logger, modbusService, tagRepo, tagService));
            }
            catch (Exception ex)
            {
                _logger.Error("Program", "Başlatma sırasında kritik hata", ex);
                MessageBox.Show($"Uygulama başlatılamadı:\n{ex.Message}", "Kritik Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
