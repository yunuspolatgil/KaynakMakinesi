using Dapper;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Infrastructure.Db;

namespace KaynakMakinesi.Infrastructure.Logging
{
    public sealed class SqliteLogSink : ILogSink
    {
        private readonly SqliteDb _db;

        public SqliteLogSink(SqliteDb db)
        {
            _db = db;
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            using (var con = _db.Open())
            {
                con.Execute(@"
CREATE TABLE IF NOT EXISTS LogEntry(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Timestamp TEXT NOT NULL,
  Level TEXT NOT NULL,
  Source TEXT,
  Message TEXT,
  Exception TEXT
);");
            }
        }

        public void Emit(LogEntry entry)
        {
            using (var con = _db.Open())
            {
                con.Execute(@"
INSERT INTO LogEntry(Timestamp, Level, Source, Message, Exception)
VALUES(@Timestamp, @Level, @Source, @Message, @Exception);",
                new
                {
                    Timestamp = entry.Timestamp.ToString("O"),
                    Level = entry.Level.ToString(),
                    entry.Source,
                    entry.Message,
                    entry.Exception
                });
            }
        }
    }
}