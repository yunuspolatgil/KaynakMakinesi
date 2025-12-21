using KaynakMakinesi.Application.Jobs;
using KaynakMakinesi.Application.Plc.Addressing;
using KaynakMakinesi.Application.Plc.Codec;
using KaynakMakinesi.Application.Plc.Service;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc.Addressing;
using KaynakMakinesi.Core.Plc.Codec;
using KaynakMakinesi.Core.Plc.Profile;
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
        private static IAppLogger _logger;

        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            System.Windows.Forms.Application.ThreadException += (s, e) =>
            {
                try { _logger?.Error("UI", "ThreadException", e.Exception); } catch { }
                MessageBox.Show(e.Exception.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try
                {
                    var ex = e.ExceptionObject as Exception;
                    _logger?.Error("APP", "UnhandledException", ex);
                }
                catch { }

                MessageBox.Show("Kritik hata oluştu. (Loglara yazıldı)", "Kritik",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try { _logger?.Error("TASK", "UnobservedTaskException", e.Exception); } catch { }
                e.SetObserved();
            };

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // --- Composition Root ---
            var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KaynakMakinesi");
            Directory.CreateDirectory(appFolder);

            var settingsPath = Path.Combine(appFolder, "appsettings.json");

            var settingsStore = new JsonFileSettingsStore(settingsPath);
            var settings = settingsStore.Load();

            var db = new SqliteDb(appFolder, settings.Database.FileName);

            // Tag repo
            var tagRepo = new SqliteTagRepository(db);
            tagRepo.EnsureSchema();

            // PLC Profile
            IPlcProfile profile = new Gmt496Profile();

            // Logger
            var inMemSink = new InMemoryLogSink(settings.Logging.KeepInMemory);
            var sqliteSink = new SqliteLogSink(db);
            IAppLogger logger = new AppLogger(inMemSink, sqliteSink);
            _logger = logger;

            // PLC + Supervisor
            var plcClient = new ModbusPlcClient(logger);
            var supervisor = new PlcConnectionSupervisor(settingsStore, plcClient, logger);

            // Jobs
            var jobRepo = new SqliteJobRepository(db);
            var runner = new JobRunner(jobRepo, supervisor, plcClient, logger);

            // Resolver + Codec + Service
            IAddressResolver resolver = new AddressResolver(profile, tagRepo);
            var codec = new ModbusCodec
            {
                SwapWordsFor32Bit = true,   // SENİN PLC İÇİN GEREKLİ
                SwapBytesInWord = false
            };
            IModbusCodec iCodec = codec;

            var modbusService = new ModbusService(plcClient, resolver, codec, settingsStore, logger);

            // Start background loops
            supervisor.Start();
            runner.Start();

            // App exit cleanup
           System.Windows.Forms.Application.ApplicationExit += (s, e) =>
            {
                try { runner.Stop(); } catch { }

                try { plcClient.DisconnectAsync(CancellationToken.None).Wait(500); } catch { }
                try { plcClient.Dispose(); } catch { }
            };

            System.Windows.Forms.Application.Run(new FrmAnaForm(settingsStore, inMemSink, supervisor, logger, modbusService, tagRepo));
        }
    }
}
