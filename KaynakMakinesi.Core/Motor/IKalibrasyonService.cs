using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KaynakMakinesi.Core.Motor
{
    /// <summary>
    /// Kalibrasyon servisi - Motor kalibrasyon iþlemlerini yönetir
    /// </summary>
    public interface IKalibrasyonService
    {
        /// <summary>
        /// Kalibrasyon kaydý ekler
        /// </summary>
        Task<bool> SaveKalibrasyonLog(KalibrasyonLog log, CancellationToken ct = default);
        
        /// <summary>
        /// Belirli motor için tüm kalibrasyon kayýtlarýný getirir
        /// </summary>
        Task<List<KalibrasyonLog>> GetKalibrasyonLogs(string motorName, CancellationToken ct = default);
        
        /// <summary>
        /// Son baþarýlý kalibrasyonu getirir
        /// </summary>
        Task<KalibrasyonLog> GetLastSuccessfulKalibrasyon(string motorName, CancellationToken ct = default);
        
        /// <summary>
        /// Tüm kalibrasyon kayýtlarýný getirir (raporlama için)
        /// </summary>
        Task<List<KalibrasyonLog>> GetAllKalibrasyonLogs(CancellationToken ct = default);
        
        /// <summary>
        /// Kalibrasyon kaydýný siler
        /// </summary>
        Task<bool> DeleteKalibrasyonLog(int id, CancellationToken ct = default);
    }
}
