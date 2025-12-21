using KaynakMakinesi.Core.Tags;
using KaynakMakinesi.Infrastructure.Db;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace KaynakMakinesi.Infrastructure.Tags
{
    public sealed class SqliteTagRepository : ITagRepository
    {
        private readonly SqliteDb _db;

        public SqliteTagRepository(SqliteDb db) { _db = db; }

        public void EnsureSchema()
        {
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                // Ensure table exists (minimal create if not)
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Tags(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL
);";
                cmd.ExecuteNonQuery();

                // Collect existing columns
                cmd.CommandText = "PRAGMA table_info('Tags');";
                var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        existing.Add(r.GetString(1)); // column name
                }

                // Add missing columns (provide defaults for NOT NULL)
                if (!existing.Contains("Address"))
                {
                    cmd.CommandText = "ALTER TABLE Tags ADD COLUMN Address TEXT NOT NULL DEFAULT '';";

                    cmd.ExecuteNonQuery();
                }
                if (!existing.Contains("Address1Based"))
                {
                    cmd.CommandText = "ALTER TABLE Tags ADD COLUMN Address1Based INTEGER NOT NULL DEFAULT 0;";
                    cmd.ExecuteNonQuery();
                }
                if (!existing.Contains("Type"))
                {
                    cmd.CommandText = "ALTER TABLE Tags ADD COLUMN Type TEXT NOT NULL DEFAULT '';";

                    cmd.ExecuteNonQuery();
                }
                if (!existing.Contains("GroupName"))
                {
                    cmd.CommandText = "ALTER TABLE Tags ADD COLUMN GroupName TEXT NULL;";
                    cmd.ExecuteNonQuery();
                }
                if (!existing.Contains("Description"))
                {
                    cmd.CommandText = "ALTER TABLE Tags ADD COLUMN Description TEXT NULL;";
                    cmd.ExecuteNonQuery();
                }
                if (!existing.Contains("PollMs"))
                {
                    cmd.CommandText = "ALTER TABLE Tags ADD COLUMN PollMs INTEGER NOT NULL DEFAULT 250;";
                    cmd.ExecuteNonQuery();
                }
                if (!existing.Contains("ReadOnly"))
                {
                    cmd.CommandText = "ALTER TABLE Tags ADD COLUMN ReadOnly INTEGER NOT NULL DEFAULT 0;";
                    cmd.ExecuteNonQuery();
                }
                if (!existing.Contains("UpdatedAt"))
                {
                    cmd.CommandText = "ALTER TABLE Tags ADD COLUMN UpdatedAt TEXT NULL;";
                    cmd.ExecuteNonQuery();
                }

                // Ensure index
                cmd.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS IX_Tags_Name ON Tags(Name);";
                cmd.ExecuteNonQuery();
            }
        }

        public List<TagDef> ListAll()
        {
            var list = new List<TagDef>();
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                // Address1Based sütununu da alıyoruz (varsayılan 0 olabilir)
                cmd.CommandText = @"SELECT Id, Name, Address, Address1Based, Type, GroupName, Description, PollMs, ReadOnly
                                    FROM Tags ORDER BY Name;";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new TagDef
                        {
                            Id = r.GetInt64(0),
                            Name = r.GetString(1),
                            Address = r.GetString(2),
                            Address1Based = r.IsDBNull(3) ? 0 : r.GetInt32(3),
                            Type = r.GetString(4),
                            GroupName = r.IsDBNull(5) ? "" : r.GetString(5),
                            Description = r.IsDBNull(6) ? "" : r.GetString(6),
                            PollMs = r.GetInt32(7),
                            ReadOnly = r.GetInt32(8) == 1
                        });
                    }
                }
            }
            return list;
        }

        public void UpsertMany(IEnumerable<TagDef> tags)
        {
            using (var con = (SQLiteConnection)_db.Open())
            using (var tx = con.BeginTransaction())
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO Tags(Name, Address, Address1Based, Type, GroupName, Description, PollMs, ReadOnly, UpdatedAt)
VALUES(@Name, @Address, @Address1Based, @Type, @GroupName, @Description, @PollMs, @ReadOnly, @UpdatedAt)
ON CONFLICT(Name) DO UPDATE SET
    Address=excluded.Address,
    Address1Based=excluded.Address1Based,
    Type=excluded.Type,
    GroupName=excluded.GroupName,
    Description=excluded.Description,
    PollMs=excluded.PollMs,
    ReadOnly=excluded.ReadOnly,
    UpdatedAt=excluded.UpdatedAt;";

                cmd.Parameters.Add(new SQLiteParameter("@Name"));
                cmd.Parameters.Add(new SQLiteParameter("@Address"));
                cmd.Parameters.Add(new SQLiteParameter("@Address1Based"));
                cmd.Parameters.Add(new SQLiteParameter("@Type"));
                cmd.Parameters.Add(new SQLiteParameter("@GroupName"));
                cmd.Parameters.Add(new SQLiteParameter("@Description"));
                cmd.Parameters.Add(new SQLiteParameter("@PollMs"));
                cmd.Parameters.Add(new SQLiteParameter("@ReadOnly"));
                cmd.Parameters.Add(new SQLiteParameter("@UpdatedAt"));

                foreach (var t in tags)
                {
                    cmd.Parameters["@Name"].Value = t.Name ?? "";
                    cmd.Parameters["@Address"].Value = t.Address ?? "";
                    cmd.Parameters["@Address1Based"].Value = t.Address1Based;
                    cmd.Parameters["@Type"].Value = t.Type ?? "";
                    cmd.Parameters["@GroupName"].Value = t.GroupName ?? "";
                    cmd.Parameters["@Description"].Value = t.Description ?? "";
                    cmd.Parameters["@PollMs"].Value = t.PollMs;
                    cmd.Parameters["@ReadOnly"].Value = t.ReadOnly ? 1 : 0;
                    cmd.Parameters["@UpdatedAt"].Value = DateTime.Now.ToString("s");
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
        }

        public void DeleteByName(string name)
        {
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Tags WHERE Name=@Name;";
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.ExecuteNonQuery();
            }
        }
    }
}