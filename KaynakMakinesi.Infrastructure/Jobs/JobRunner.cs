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

            _cts = new CancellationTokenSource();
            _loop = Task.Run(() => RunAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private async Task RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // bağlantı yoksa job çalıştırma (kuyruk bekler, kopunca kaybolmaz)
                if (_conn.State != ConnectionState.Connected)
                {
                    await Task.Delay(250, ct);
                    continue;
                }

                var job = _repo.DequeueNextPending();
                if (job == null)
                {
                    await Task.Delay(200, ct);
                    continue;
                }

                try
                {
                    await ExecuteJobAsync(job, ct);
                    job.State = JobState.Done;
                    job.LastError = null;
                    _repo.Update(job);

                    _log.Info(nameof(JobRunner), $"Job DONE Id={job.Id} Type={job.Type}");
                }
                catch (Exception ex)
                {
                    job.RetryCount++;
                    job.LastError = ex.ToString();
                    job.State = (job.RetryCount >= 5) ? JobState.Failed : JobState.Pending; // retry policy
                    _repo.Update(job);

                    _log.Error(nameof(JobRunner), $"Job FAIL Id={job.Id} Type={job.Type} Retry={job.RetryCount}", ex);
                    await Task.Delay(300, ct);
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
                await _plc.WriteSingleRegisterAsync(p.UnitId, p.Address, p.Value, ct);
                return;
            }

            throw new NotSupportedException("Unknown job type: " + job.Type);
        }

        private sealed class WriteRegisterPayload
        {
            public byte UnitId { get; set; }
            public ushort Address { get; set; }
            public ushort Value { get; set; }
        }
    }
}
