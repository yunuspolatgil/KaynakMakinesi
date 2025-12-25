using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using KaynakMakinesi.Core.Entities;
using KaynakMakinesi.Core.Repositories;
using KaynakMakinesi.Infrastructure.Db;
using Newtonsoft.Json;

namespace KaynakMakinesi.Infrastructure.Repositories
{
    /// <summary>
    /// SQLite ile TagEntity CRUD iþlemleri
    /// Dinamik MetadataJson kolonu sayesinde kullanýcý istediði kadar alan ekleyebilir
    /// </summary>
    public sealed class SqliteTagEntityRepository : ITagEntityRepository
    {
        private readonly SqliteDb _db;
        
        public SqliteTagEntityRepository(SqliteDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            EnsureSchema();
        }
        
        #region Schema Management
        
        /// <summary>
        /// TagEntities tablosunu oluþturur/günceller
        /// </summary>
        private void EnsureSchema()
        {
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                // Ana tablo oluþtur
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS TagEntities(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE COLLATE NOCASE,
    Address TEXT NOT NULL,
    DataType TEXT NOT NULL DEFAULT 'UShort',
    GroupName TEXT NULL,
    MetadataJson TEXT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedBy TEXT NULL,
    UpdatedBy TEXT NULL
);";
                cmd.ExecuteNonQuery();
                
                // Index'ler
                cmd.CommandText = @"
CREATE INDEX IF NOT EXISTS IX_TagEntities_Name ON TagEntities(Name COLLATE NOCASE);
CREATE INDEX IF NOT EXISTS IX_TagEntities_GroupName ON TagEntities(GroupName);
CREATE INDEX IF NOT EXISTS IX_TagEntities_DataType ON TagEntities(DataType);
CREATE INDEX IF NOT EXISTS IX_TagEntities_IsActive ON TagEntities(IsActive);";
                cmd.ExecuteNonQuery();
            }
        }
        
        #endregion
        
        #region IRepository<TagEntity> Implementation
        
        public TagEntity GetById(long id)
        {
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT Id, Name, Address, DataType, GroupName, MetadataJson, 
       CreatedAt, UpdatedAt, IsActive, CreatedBy, UpdatedBy
FROM TagEntities
WHERE Id = @Id AND IsActive = 1;";
                cmd.Parameters.AddWithValue("@Id", id);
                
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                        return MapFromReader(r);
                }
            }
            return null;
        }
        
        public IEnumerable<TagEntity> GetAll()
        {
            var list = new List<TagEntity>();
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT Id, Name, Address, DataType, GroupName, MetadataJson, 
       CreatedAt, UpdatedAt, IsActive, CreatedBy, UpdatedBy
FROM TagEntities
WHERE IsActive = 1
ORDER BY Name COLLATE NOCASE;";
                
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(MapFromReader(r));
                }
            }
            
            return list;
        }
        
        public IEnumerable<TagEntity> Find(Expression<Func<TagEntity, bool>> predicate)
        {
            // SQLite için in-memory filtering (Expression to SQL dönüþümü kompleks)
            var all = GetAll();
            var compiled = predicate.Compile();
            return all.Where(compiled);
        }
        
        public TagEntity SingleOrDefault(Expression<Func<TagEntity, bool>> predicate)
        {
            return Find(predicate).FirstOrDefault();
        }
        
        public void Add(TagEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            if (!entity.Validate(out var error))
                throw new InvalidOperationException(error);
            
            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;
            entity.IsActive = true;
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO TagEntities(Name, Address, DataType, GroupName, MetadataJson, 
                        CreatedAt, UpdatedAt, IsActive, CreatedBy, UpdatedBy)
VALUES(@Name, @Address, @DataType, @GroupName, @MetadataJson, 
       @CreatedAt, @UpdatedAt, @IsActive, @CreatedBy, @UpdatedBy);
SELECT last_insert_rowid();";
                
                AddParameters(cmd, entity);
                
                var id = (long)cmd.ExecuteScalar();
                entity.Id = id;
            }
        }
        
        public void AddRange(IEnumerable<TagEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    foreach (var entity in entities)
                    {
                        if (!entity.Validate(out var error))
                            throw new InvalidOperationException(error);
                        
                        entity.CreatedAt = DateTime.Now;
                        entity.UpdatedAt = DateTime.Now;
                        entity.IsActive = true;
                        
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.CommandText = @"
INSERT INTO TagEntities(Name, Address, DataType, GroupName, MetadataJson, 
                        CreatedAt, UpdatedAt, IsActive, CreatedBy, UpdatedBy)
VALUES(@Name, @Address, @DataType, @GroupName, @MetadataJson, 
       @CreatedAt, @UpdatedAt, @IsActive, @CreatedBy, @UpdatedBy);";
                            
                            AddParameters(cmd, entity);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
        
        public void Update(TagEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            if (!entity.Validate(out var error))
                throw new InvalidOperationException(error);
            
            entity.UpdatedAt = DateTime.Now;
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE TagEntities
SET Name = @Name,
    Address = @Address,
    DataType = @DataType,
    GroupName = @GroupName,
    MetadataJson = @MetadataJson,
    UpdatedAt = @UpdatedAt,
    UpdatedBy = @UpdatedBy
WHERE Id = @Id;";
                
                AddParameters(cmd, entity);
                
                var affected = cmd.ExecuteNonQuery();
                if (affected == 0)
                    throw new InvalidOperationException($"TagEntity with Id={entity.Id} not found.");
            }
        }
        
        public void Remove(TagEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            // Soft delete
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE TagEntities
SET IsActive = 0,
    UpdatedAt = @UpdatedAt
WHERE Id = @Id;";
                
                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("O"));
                
                cmd.ExecuteNonQuery();
            }
        }
        
        public void RemovePermanently(TagEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM TagEntities WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void RemoveRange(IEnumerable<TagEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var tx = con.BeginTransaction())
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
UPDATE TagEntities
SET IsActive = 0,
    UpdatedAt = @UpdatedAt
WHERE Id = @Id;";
                
                cmd.Parameters.Add(new SQLiteParameter("@Id"));
                cmd.Parameters.Add(new SQLiteParameter("@UpdatedAt"));
                
                try
                {
                    foreach (var entity in entities)
                    {
                        cmd.Parameters["@Id"].Value = entity.Id;
                        cmd.Parameters["@UpdatedAt"].Value = DateTime.Now.ToString("O");
                        cmd.ExecuteNonQuery();
                    }
                    
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
        
        public int Count()
        {
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM TagEntities WHERE IsActive = 1;";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        
        public int Count(Expression<Func<TagEntity, bool>> predicate)
        {
            return Find(predicate).Count();
        }
        
        public bool Any(Expression<Func<TagEntity, bool>> predicate)
        {
            return Find(predicate).Any();
        }
        
        #endregion
        
        #region ITagEntityRepository Implementation
        
        public TagEntity GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT Id, Name, Address, DataType, GroupName, MetadataJson, 
       CreatedAt, UpdatedAt, IsActive, CreatedBy, UpdatedBy
FROM TagEntities
WHERE Name = @Name COLLATE NOCASE AND IsActive = 1;";
                
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                        return MapFromReader(r);
                }
            }
            
            return null;
        }
        
        public IEnumerable<TagEntity> GetByGroup(string groupName)
        {
            var list = new List<TagEntity>();
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT Id, Name, Address, DataType, GroupName, MetadataJson, 
       CreatedAt, UpdatedAt, IsActive, CreatedBy, UpdatedBy
FROM TagEntities
WHERE GroupName = @GroupName AND IsActive = 1
ORDER BY Name COLLATE NOCASE;";
                
                cmd.Parameters.AddWithValue("@GroupName", groupName ?? "");
                
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(MapFromReader(r));
                }
            }
            
            return list;
        }
        
        public IEnumerable<TagEntity> GetByDataType(string dataType)
        {
            var list = new List<TagEntity>();
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT Id, Name, Address, DataType, GroupName, MetadataJson, 
       CreatedAt, UpdatedAt, IsActive, CreatedBy, UpdatedBy
FROM TagEntities
WHERE DataType = @DataType AND IsActive = 1
ORDER BY Name COLLATE NOCASE;";
                
                cmd.Parameters.AddWithValue("@DataType", dataType ?? "");
                
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(MapFromReader(r));
                }
            }
            
            return list;
        }
        
        public bool ExistsByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT COUNT(*) FROM TagEntities 
WHERE Name = @Name COLLATE NOCASE AND IsActive = 1;";
                
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
        
        public void RemoveByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE TagEntities
SET IsActive = 0,
    UpdatedAt = @UpdatedAt
WHERE Name = @Name COLLATE NOCASE;";
                
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("O"));
                
                cmd.ExecuteNonQuery();
            }
        }
        
        public void RemoveByNames(IEnumerable<string> names)
        {
            if (names == null || !names.Any())
                return;
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var tx = con.BeginTransaction())
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
UPDATE TagEntities
SET IsActive = 0,
    UpdatedAt = @UpdatedAt
WHERE Name = @Name COLLATE NOCASE;";
                
                cmd.Parameters.Add(new SQLiteParameter("@Name"));
                cmd.Parameters.Add(new SQLiteParameter("@UpdatedAt"));
                
                try
                {
                    foreach (var name in names)
                    {
                        if (string.IsNullOrWhiteSpace(name))
                            continue;
                        
                        cmd.Parameters["@Name"].Value = name.Trim();
                        cmd.Parameters["@UpdatedAt"].Value = DateTime.Now.ToString("O");
                        cmd.ExecuteNonQuery();
                    }
                    
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
        
        public void UpsertMany(IEnumerable<TagEntity> tags)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    foreach (var tag in tags)
                    {
                        if (!tag.Validate(out var error))
                            throw new InvalidOperationException(error);
                        
                        tag.UpdatedAt = DateTime.Now;
                        
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.CommandText = @"
INSERT INTO TagEntities(Name, Address, DataType, GroupName, MetadataJson, 
                        CreatedAt, UpdatedAt, IsActive, CreatedBy, UpdatedBy)
VALUES(@Name, @Address, @DataType, @GroupName, @MetadataJson, 
       @CreatedAt, @UpdatedAt, @IsActive, @CreatedBy, @UpdatedBy)
ON CONFLICT(Name) DO UPDATE SET
    Address = excluded.Address,
    DataType = excluded.DataType,
    GroupName = excluded.GroupName,
    MetadataJson = excluded.MetadataJson,
    UpdatedAt = excluded.UpdatedAt,
    UpdatedBy = excluded.UpdatedBy;";
                            
                            if (tag.CreatedAt == default(DateTime))
                                tag.CreatedAt = DateTime.Now;
                            
                            AddParameters(cmd, tag);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
        
        public IEnumerable<string> GetAllGroups()
        {
            var groups = new List<string>();
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT DISTINCT GroupName 
FROM TagEntities 
WHERE IsActive = 1 AND GroupName IS NOT NULL AND GroupName != ''
ORDER BY GroupName COLLATE NOCASE;";
                
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var group = r.GetString(0);
                        if (!string.IsNullOrWhiteSpace(group))
                            groups.Add(group);
                    }
                }
            }
            
            return groups;
        }
        
        public IEnumerable<TagEntity> GetInactive()
        {
            var list = new List<TagEntity>();
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT Id, Name, Address, DataType, GroupName, MetadataJson, 
       CreatedAt, UpdatedAt, IsActive, CreatedBy, UpdatedBy
FROM TagEntities
WHERE IsActive = 0
ORDER BY UpdatedAt DESC;";
                
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(MapFromReader(r));
                }
            }
            
            return list;
        }
        
        public void Restore(long id)
        {
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE TagEntities
SET IsActive = 1,
    UpdatedAt = @UpdatedAt
WHERE Id = @Id;";
                
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("O"));
                
                cmd.ExecuteNonQuery();
            }
        }
        
        /// <summary>
        /// ?? Grup ve suffix'e göre tag getirir
        /// Örnek: GetByGroupAndSuffix("Motor_K0", "Home_Hiz") 
        ///   -> "K0_Home_Hiz", "Motor_K0_Home_Hiz", "Home_Hiz" gibi tag'leri bulur
        /// 
        /// ?? FÝX: Suffix contains aramasý yapýlýr (case-insensitive)
        /// Örnek: suffix="Ileri" -> "K0_Ileri", "K0_Ileri_Hiz", "Motor_K0_Ileri" hepsi bulunur
        /// </summary>
        public TagEntity GetByGroupAndSuffix(string groupName, string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                return null;
            
            // Önce grup içinde suffix ile biten tag'leri ara
            var groupTags = GetByGroup(groupName);
            
            if (!groupTags.Any())
            {
                // Grup boþ, hiç tag yok
                return null;
            }
            
            var suffixLower = suffix.ToLowerInvariant();
            
            // ONCELIK 1: Tam eþleþme (case-insensitive)
            // Örnek: suffix="Ileri" -> Name="Ileri" (birebir eþleþme)
            var exactMatch = groupTags.FirstOrDefault(t => 
                t.Name.Equals(suffix, StringComparison.OrdinalIgnoreCase));
            
            if (exactMatch != null)
                return exactMatch;
            
            // ONCELIK 2: Suffix ile biten tag'ler (underscore ile)
            // Örnek: suffix="Ileri" -> Name="K0_Ileri" ?
            var endsWith = groupTags.FirstOrDefault(t => 
                t.Name.ToLowerInvariant().EndsWith("_" + suffixLower));
            
            if (endsWith != null)
                return endsWith;
            
            // ONCELIK 3: Contains (içerir) - en esnek
            // Örnek: suffix="Ileri" -> Name="K0_Ileri_Hiz" ?
            //        suffix="Home" -> Name="K0_Home_Git" ?
            // ?? Ancak: Önce underscore ile baþlayan içerenleri tercih et
            //    Örnek: suffix="Home" -> "_Home_" içeren önce bulunur
            var containsWithUnderscore = groupTags.FirstOrDefault(t => 
                t.Name.ToLowerInvariant().Contains("_" + suffixLower + "_") ||
                t.Name.ToLowerInvariant().Contains("_" + suffixLower));
            
            if (containsWithUnderscore != null)
                return containsWithUnderscore;
            
            // ONCELIK 4: Contains (herhangi bir yerde)
            return groupTags.FirstOrDefault(t => 
                t.Name.ToLowerInvariant().Contains(suffixLower));
        }
        
        /// <summary>
        /// ?? Ýsmin sonunda verilen suffix'i içeren tag'leri getirir
        /// Örnek: GetBySuffix("Home_Hiz") -> "K0_Home_Hiz", "K1_Home_Hiz", vb.
        /// </summary>
        public IEnumerable<TagEntity> GetBySuffix(string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                return Enumerable.Empty<TagEntity>();
            
            var list = new List<TagEntity>();
            
            using (var con = (SQLiteConnection)_db.Open())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT Id, Name, Address, DataType, GroupName, MetadataJson, 
       CreatedAt, UpdatedAt, IsActive, CreatedBy, UpdatedBy
FROM TagEntities
WHERE (Name LIKE '%' || @Suffix COLLATE NOCASE) AND IsActive = 1
ORDER BY Name COLLATE NOCASE;";
                
                cmd.Parameters.AddWithValue("@Suffix", suffix.Trim());
                
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(MapFromReader(r));
                }
            }
            
            return list;
        }
        
        #endregion
        
        #region Helper Methods
        
        private void AddParameters(SQLiteCommand cmd, TagEntity entity)
        {
            cmd.Parameters.AddWithValue("@Id", entity.Id);
            cmd.Parameters.AddWithValue("@Name", entity.Name ?? "");
            cmd.Parameters.AddWithValue("@Address", entity.Address ?? "");
            cmd.Parameters.AddWithValue("@DataType", entity.DataType ?? "UShort");
            cmd.Parameters.AddWithValue("@GroupName", entity.GroupName ?? "");
            cmd.Parameters.AddWithValue("@MetadataJson", entity.MetadataJson ?? "{}");
            cmd.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt.ToString("O"));
            cmd.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt.ToString("O"));
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy ?? "");
            cmd.Parameters.AddWithValue("@UpdatedBy", entity.UpdatedBy ?? "");
        }
        
        private TagEntity MapFromReader(SQLiteDataReader r)
        {
            return new TagEntity
            {
                Id = r.GetInt64(0),
                Name = r.GetString(1),
                Address = r.GetString(2),
                DataType = r.GetString(3),
                GroupName = r.IsDBNull(4) ? "" : r.GetString(4),
                MetadataJson = r.IsDBNull(5) ? "{}" : r.GetString(5),
                CreatedAt = DateTime.Parse(r.GetString(6)),
                UpdatedAt = DateTime.Parse(r.GetString(7)),
                IsActive = r.GetInt32(8) == 1,
                CreatedBy = r.IsDBNull(9) ? "" : r.GetString(9),
                UpdatedBy = r.IsDBNull(10) ? "" : r.GetString(10)
            };
        }
        
        #endregion
    }
}
