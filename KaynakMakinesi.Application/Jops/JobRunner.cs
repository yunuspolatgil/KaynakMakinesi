using System;
using System.Threading;
using System.Threading.Tasks;
using KaynakMakinesi.Core.Jobs;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc;
using Newtonsoft.Json;


namespace KaynakMakinesi.Application.Jobs
{
    public sealed class JobRunner
    {
        private readonly IJobRepository _repo;
        private readonly IConnectionSupervisor _conn;
        private readonly IPlcClient _plc;
        private readonly IAppLogger _log;

        private CancellationTokenSource _cts;
        private Task _loop;

        public JobRunner(IJobRepository repo, IConnectionSupervisor conn, IPlcClient plc, IAppLogger log)
        {
            _repo = repo;
            _conn = conn;
            _plc = plc;
            _log = log;
        }

        public void Start()
        {
            _repo.EnsureSchema();
            _repo.MarkStaleInProgressAsPending(TimeSpan.FromMinutes(5)); // app kapandıysa kaldığı yerden devam

            if (_cts != null) return;
            _cts = new CancellationTokenSource();

            // Run the loop in a resilient background task so unexpected exceptions don't terminate the process
            var task = Task.Run(async () =>
            {
                var ct = _cts.Token;
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        await RunAsync(ct).ConfigureAwait(false);
                        // normally RunAsync only returns on cancellation
                        break;
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        try { _log.Error(nameof(JobRunner), "JobRunner döngüsünde hata; yeniden başlatılacak", ex); } catch { }
                        try { await Task.Delay(1000, CancellationToken.None).ConfigureAwait(false); } catch { }
                    }
                }
            });

            _loop = task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    try { _log.Error(nameof(JobRunner), "JobRunner arka plan görevi hata verdi", t.Exception); } catch { }
                }
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
        }

        public void Stop()
        {
            try { _cts?.Cancel(); } catch { }
            try { _loop?.Wait(500); } catch { }
            _cts = null;
            _loop = null;
        }

        private async Task RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // bağlantı yoksa job çalıştırma (kuyruk bekler, kopunca kaybolmaz)
                if (_conn.State != ConnectionState.Connected)
                {
                    try { await Task.Delay(250, ct).ConfigureAwait(false); } catch { }
                    continue;
                }

                var job = _repo.DequeueNextPending();
                if (job == null)
                {
                    try { await Task.Delay(200, ct).ConfigureAwait(false); } catch { }
                    continue;
                }

                try
                {
                    await ExecuteJobAsync(job, ct).ConfigureAwait(false);
                    job.State = JobState.Done;
                    job.LastError = null;
                    _repo.Update(job);

                    _log.Info(nameof(JobRunner), $"Job TAMAMLANDI Id={job.Id} Tip={job.Type}");
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // shutdown requested
                    break;
                }
                catch (Exception ex)
                {
                    job.RetryCount++;
                    job.LastError = ex.ToString();
                    job.State = (job.RetryCount >= 5) ? JobState.Failed : JobState.Pending; // retry policy
                    _repo.Update(job);

                    _log.Error(nameof(JobRunner), $"Job HATA Id={job.Id} Tip={job.Type} Deneme={job.RetryCount}", ex);
                    try { await Task.Delay(300, ct).ConfigureAwait(false); } catch { }
                }
            }
        }

        // ÖRNEK: "WriteRegister" job'u
        private async Task ExecuteJobAsync(JobItem job, CancellationToken ct)
        {
            if (job.Type == "WriteRegister")
            {
                var p = JsonConvert.DeserializeObject<WriteRegisterPayload>(job.PayloadJson);
                // Not: unitId ayarlardan da alınabilir; burada payload’a koyduk
                await _plc.WriteSingleRegisterAsync(p.UnitId, p.Address, p.Value, ct).ConfigureAwait(false);
                return;
            }

            throw new NotSupportedException("Bilinmeyen job tipi: " + job.Type);
        }

        private sealed class WriteRegisterPayload
        {
            public byte UnitId { get; set; }
            public ushort Address { get; set; }
            public ushort Value { get; set; }
        }
    }
}
