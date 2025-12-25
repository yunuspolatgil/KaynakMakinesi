using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KaynakMakinesi.Core.Motor;

namespace KaynakMakinesi.Infrastructure.Motor
{
    /// <summary>
    /// Motor kalibrasyon servisi - Ýþ mantýðý katmaný
    /// </summary>
    public class MotorKalibrasyonService : IKalibrasyonService
    {
        private readonly KalibrasyonRepository _repository;

        public MotorKalibrasyonService(KalibrasyonRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> SaveKalibrasyonLog(KalibrasyonLog log, CancellationToken ct = default)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            if (string.IsNullOrWhiteSpace(log.MotorName))
                throw new ArgumentException("Motor adý boþ olamaz", nameof(log));

            // Timestamp set edilmemiþse þimdi yap
            if (log.Timestamp == default)
                log.Timestamp = DateTime.Now;

            var id = await _repository.InsertAsync(log, ct).ConfigureAwait(false);
            return id > 0;
        }

        public async Task<List<KalibrasyonLog>> GetKalibrasyonLogs(string motorName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(motorName))
                throw new ArgumentException("Motor adý boþ olamaz", nameof(motorName));

            return await _repository.GetByMotorAsync(motorName, ct).ConfigureAwait(false);
        }

        public async Task<KalibrasyonLog> GetLastSuccessfulKalibrasyon(string motorName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(motorName))
                throw new ArgumentException("Motor adý boþ olamaz", nameof(motorName));

            return await _repository.GetLastSuccessfulAsync(motorName, ct).ConfigureAwait(false);
        }

        public async Task<List<KalibrasyonLog>> GetAllKalibrasyonLogs(CancellationToken ct = default)
        {
            return await _repository.GetAllAsync(ct).ConfigureAwait(false);
        }

        public async Task<bool> DeleteKalibrasyonLog(int id, CancellationToken ct = default)
        {
            if (id <= 0)
                throw new ArgumentException("Geçersiz ID", nameof(id));

            return await _repository.DeleteAsync(id, ct).ConfigureAwait(false);
        }
    }
}
