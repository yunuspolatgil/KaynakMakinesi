using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace KaynakMakinesi.Core.Repositories
{
    /// <summary>
    /// Generic Repository Interface - Tüm entity'ler için temel CRUD iþlemleri
    /// </summary>
    /// <typeparam name="TEntity">Entity tipi (EntityBase'den türemiþ olmalý)</typeparam>
    public interface IRepository<TEntity> where TEntity : Entities.EntityBase
    {
        #region CRUD Ýþlemleri
        
        /// <summary>
        /// ID'ye göre entity getirir
        /// </summary>
        TEntity GetById(long id);
        
        /// <summary>
        /// Tüm entity'leri getirir (aktif olanlar)
        /// </summary>
        IEnumerable<TEntity> GetAll();
        
        /// <summary>
        /// Filtre ile entity'leri getirir
        /// </summary>
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        
        /// <summary>
        /// Tek bir entity getirir (predicate ile)
        /// </summary>
        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate);
        
        /// <summary>
        /// Yeni entity ekler
        /// </summary>
        void Add(TEntity entity);
        
        /// <summary>
        /// Birden fazla entity ekler
        /// </summary>
        void AddRange(IEnumerable<TEntity> entities);
        
        /// <summary>
        /// Entity günceller
        /// </summary>
        void Update(TEntity entity);
        
        /// <summary>
        /// Entity siler (soft delete - IsActive = false)
        /// </summary>
        void Remove(TEntity entity);
        
        /// <summary>
        /// Entity'yi kalýcý olarak siler (hard delete)
        /// </summary>
        void RemovePermanently(TEntity entity);
        
        /// <summary>
        /// Birden fazla entity siler (soft delete)
        /// </summary>
        void RemoveRange(IEnumerable<TEntity> entities);
        
        #endregion
        
        #region Query Ýþlemleri
        
        /// <summary>
        /// Entity sayýsýný döndürür
        /// </summary>
        int Count();
        
        /// <summary>
        /// Filtreye uyan entity sayýsýný döndürür
        /// </summary>
        int Count(Expression<Func<TEntity, bool>> predicate);
        
        /// <summary>
        /// Herhangi bir entity var mý?
        /// </summary>
        bool Any(Expression<Func<TEntity, bool>> predicate);
        
        #endregion
    }
}
