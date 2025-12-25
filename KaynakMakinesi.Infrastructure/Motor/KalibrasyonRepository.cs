using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KaynakMakinesi.Core.Motor;

namespace KaynakMakinesi.Infrastructure.Motor
{
    /// <summary>
    /// Kalibrasyon kayýtlarý için SQLite repository
    /// </summary>
    public class KalibrasyonRepository
    {
        private readonly string _connectionString;

        public KalibrasyonRepository(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS KalibrasyonLogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        MotorName TEXT NOT NULL,
                        Timestamp TEXT NOT NULL,
                        KullaniciAdi TEXT,
                        OlculenPozisyon REAL NOT NULL,
                        CikisPozisyonu REAL NOT NULL,
                        RampaHizlanma REAL NOT NULL,
                        RampaYavaslama REAL NOT NULL,
                        TestPozisyon REAL,
                        TestBasarili INTEGER NOT NULL,
                        Basarili INTEGER NOT NULL,
                        HataMesaji TEXT,
                        Notlar TEXT
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_motor_timestamp 
                    ON KalibrasyonLogs(MotorName, Timestamp DESC);
                ";
                cmd.ExecuteNonQuery();
            }
        }

        public async Task<int> InsertAsync(KalibrasyonLog log, CancellationToken ct = default)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync(ct).ConfigureAwait(false);
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO KalibrasyonLogs 
                    (MotorName, Timestamp, KullaniciAdi, OlculenPozisyon, CikisPozisyonu, 
                     RampaHizlanma, RampaYavaslama, TestPozisyon, TestBasarili, Basarili, 
                     HataMesaji, Notlar)
                    VALUES 
                    (@MotorName, @Timestamp, @KullaniciAdi, @OlculenPozisyon, @CikisPozisyonu,
                     @RampaHizlanma, @RampaYavaslama, @TestPozisyon, @TestBasarili, @Basarili,
                     @HataMesaji, @Notlar);
                    
                    SELECT last_insert_rowid();
                ";
                
                cmd.Parameters.AddWithValue("@MotorName", log.MotorName);
                cmd.Parameters.AddWithValue("@Timestamp", log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@KullaniciAdi", log.KullaniciAdi ?? string.Empty);
                cmd.Parameters.AddWithValue("@OlculenPozisyon", log.OlculenPozisyon);
                cmd.Parameters.AddWithValue("@CikisPozisyonu", log.CikisPozisyonu);
                cmd.Parameters.AddWithValue("@RampaHizlanma", log.RampaHizlanma);
                cmd.Parameters.AddWithValue("@RampaYavaslama", log.RampaYavaslama);
                cmd.Parameters.AddWithValue("@TestPozisyon", log.TestPozisyon.HasValue ? (object)log.TestPozisyon.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@TestBasarili", log.TestBasarili ? 1 : 0);
                cmd.Parameters.AddWithValue("@Basarili", log.Basarili ? 1 : 0);
                cmd.Parameters.AddWithValue("@HataMesaji", log.HataMesaji ?? string.Empty);
                cmd.Parameters.AddWithValue("@Notlar", log.Notlar ?? string.Empty);
                
                var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                return Convert.ToInt32(result);
            }
        }

        public async Task<List<KalibrasyonLog>> GetByMotorAsync(string motorName, CancellationToken ct = default)
        {
            var logs = new List<KalibrasyonLog>();
            
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync(ct).ConfigureAwait(false);
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT * FROM KalibrasyonLogs 
                    WHERE MotorName = @MotorName 
                    ORDER BY Timestamp DESC
                ";
                cmd.Parameters.AddWithValue("@MotorName", motorName);
                
                using (var reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(ct).ConfigureAwait(false))
                    {
                        logs.Add(ReadLog(reader));
                    }
                }
            }
            
            return logs;
        }

        public async Task<KalibrasyonLog> GetLastSuccessfulAsync(string motorName, CancellationToken ct = default)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync(ct).ConfigureAwait(false);
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT * FROM KalibrasyonLogs 
                    WHERE MotorName = @MotorName AND Basarili = 1
                    ORDER BY Timestamp DESC
                    LIMIT 1
                ";
                cmd.Parameters.AddWithValue("@MotorName", motorName);
                
                using (var reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync(ct).ConfigureAwait(false))
                    {
                        return ReadLog(reader);
                    }
                }
            }
            
            return null;
        }

        public async Task<List<KalibrasyonLog>> GetAllAsync(CancellationToken ct = default)
        {
            var logs = new List<KalibrasyonLog>();
            
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync(ct).ConfigureAwait(false);
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM KalibrasyonLogs ORDER BY Timestamp DESC";
                
                using (var reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(ct).ConfigureAwait(false))
                    {
                        logs.Add(ReadLog(reader));
                    }
                }
            }
            
            return logs;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync(ct).ConfigureAwait(false);
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM KalibrasyonLogs WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Id", id);
                
                var affected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                return affected > 0;
            }
        }

        private KalibrasyonLog ReadLog(SQLiteDataReader reader)
        {
            return new KalibrasyonLog
            {
                Id = reader.GetInt32(0),
                MotorName = reader.GetString(1),
                Timestamp = DateTime.Parse(reader.GetString(2)),
                KullaniciAdi = reader.IsDBNull(3) ? null : reader.GetString(3),
                OlculenPozisyon = (float)reader.GetDouble(4),
                CikisPozisyonu = (float)reader.GetDouble(5),
                RampaHizlanma = (float)reader.GetDouble(6),
                RampaYavaslama = (float)reader.GetDouble(7),
                TestPozisyon = reader.IsDBNull(8) ? (float?)null : (float)reader.GetDouble(8),
                TestBasarili = reader.GetInt32(9) == 1,
                Basarili = reader.GetInt32(10) == 1,
                HataMesaji = reader.IsDBNull(11) ? null : reader.GetString(11),
                Notlar = reader.IsDBNull(12) ? null : reader.GetString(12)
            };
        }
    }
}
