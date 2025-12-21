using System.Collections.Generic;
using System.Linq;
using Dapper;
using KaynakMakinesi.Core.Tags;
using KaynakMakinesi.Infrastructure.Db;

namespace KaynakMakinesi.Infrastructure.Tags
{
    public sealed class SqliteTagRepository : ITagRepository
    {
        private readonly SqliteDb _db;

        public SqliteTagRepository(SqliteDb db)
        {
            _db = db;
        }

        public void EnsureSchema()
        {
            using (var con = _db.Open())
            {
                con.Execute(@"
CREATE TABLE IF NOT EXISTS Tags(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL UNIQUE,
  Address1Based INTEGER NOT NULL,
  Description TEXT,
  ReadOnly INTEGER NOT NULL DEFAULT 0,
  AreaOverride TEXT,
  TypeOverride TEXT,
  Scale REAL NOT NULL DEFAULT 1,
  Offset REAL NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS IX_Tags_Name ON Tags(Name);
");
            }
        }

        public bool TryGetByName(string name, out TagDefinition tag)
        {
            tag = null;
            if (string.IsNullOrWhiteSpace(name)) return false;

            using (var con = _db.Open())
            {
                tag = con.Query<TagDefinition>(
                    "SELECT * FROM Tags WHERE Name = @Name LIMIT 1;",
                    new { Name = name.Trim() }).FirstOrDefault();
            }

            return tag != null;
        }

        public List<TagDefinition> GetAll()
        {
            using (var con = _db.Open())
                return con.Query<TagDefinition>("SELECT * FROM Tags ORDER BY Name;").ToList();
        }

        public void Upsert(TagDefinition t)
        {
            using (var con = _db.Open())
            {
                con.Execute(@"
INSERT INTO Tags(Name, Address1Based, Description, ReadOnly, AreaOverride, TypeOverride, Scale, Offset)
VALUES(@Name,@Address1Based,@Description,@ReadOnly,@AreaOverride,@TypeOverride,@Scale,@Offset)
ON CONFLICT(Name) DO UPDATE SET
 Address1Based=excluded.Address1Based,
 Description=excluded.Description,
 ReadOnly=excluded.ReadOnly,
 AreaOverride=excluded.AreaOverride,
 TypeOverride=excluded.TypeOverride,
 Scale=excluded.Scale,
 Offset=excluded.Offset;
", t);
            }
        }

        public void Delete(long id)
        {
            using (var con = _db.Open())
                con.Execute("DELETE FROM Tags WHERE Id=@Id;", new { Id = id });
        }
    }
}
