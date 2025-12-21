using System;
using System.IO;
using System.Windows.Forms;
using KaynakMakinesi.Application.Jobs;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Settings;
using KaynakMakinesi.Infrastructure.Db;
using KaynakMakinesi.Infrastructure.Jobs;
using KaynakMakinesi.Infrastructure.Logging;
using KaynakMakinesi.Infrastructure.Plc;
using KaynakMakinesi.Infrastructure.Settings;

namespace KaynakMakinesi.UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
          
            System.Windows.Forms.Application.ThreadException += (s, e) =>
            {
                // burada logger henüz kurulmadıysa bile en azından messagebox/log dosyası yapılabilir
                MessageBox.Show(e.Exception.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                MessageBox.Show("Kritik hata: " + e.ExceptionObject, "Kritik", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            //SQLitePCL.Batteries.Init();
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // --- Composition Root ---
            var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KaynakMakinesi");
            var settingsPath = Path.Combine(appFolder, "appsettings.json");

            var settingsStore = new JsonFileSettingsStore(settingsPath);
            var settings = settingsStore.Load();

            var db = new SqliteDb(appFolder, settings.Database.FileName);

            var inMemSink = new InMemoryLogSink(settings.Logging.KeepInMemory);
            var sqliteSink = new SqliteLogSink(db);
            IAppLogger logger = new AppLogger(inMemSink, sqliteSink);

            var plcClient = new ModbusPlcClient();
            var supervisor = new PlcConnectionSupervisor(settingsStore, plcClient, logger);

            var jobRepo = new SqliteJobRepository(db);
            var runner = new JobRunner(jobRepo, supervisor, plcClient, logger);

            supervisor.Start();
            runner.Start();

            System.Windows.Forms.Application.Run(new MainForm(settingsStore, inMemSink, supervisor, logger));
        }
    }
}
