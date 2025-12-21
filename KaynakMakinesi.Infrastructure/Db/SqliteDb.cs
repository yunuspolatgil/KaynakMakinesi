using System.Data;
using System.Data.SQLite;
using System.IO;

namespace KaynakMakinesi.Infrastructure.Db
{
    public sealed class SqliteDb
    {
        public string DbPath { get; }
        public string ConnectionString { get; }

        public SqliteDb(string folder, string fileName)
        {
            Directory.CreateDirectory(folder);
            DbPath = Path.Combine(folder, fileName);

            // System.Data.SQLite bu keyword'leri destekler
            ConnectionString =
                $"Data Source={DbPath};Version=3;Pooling=True;Journal Mode=WAL;Synchronous=NORMAL;";
        }

        public IDbConnection Open()
        {
            var con = new SQLiteConnection(ConnectionString);
            con.Open();

            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys=ON;";
                cmd.ExecuteNonQuery();
            }

            return con;
        }
    }
}
