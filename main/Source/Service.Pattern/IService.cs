using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Repository.Pattern.Repositories;
using TrackableEntities;

namespace Service.Pattern
{
    public interface IService<TEntity> where TEntity : ITrackable
    {
        TEntity Find(params object[] keyValues);
        IQueryable<TEntity> SelectQuery(string query, params object[] parameters);
        void Insert(TEntity entity);
        void InsertRange(IEnumerable<TEntity> entities);
        void ApplyChanges(TEntity entity);
        [Obsolete("InsertOrUpdateGraph has been deprecated.  Instead set TrackingState to Added or Modified and call ApplyChanges.")]
        void InsertOrUpdateGraph(TEntity entity);
        [Obsolete("InsertGraphRange has been deprecated. Instead call Insert to set TrackingState on enttites in a graph.")]
        void InsertGraphRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Delete(object id);
        void Delete(TEntity entity);
        IQueryFluent<TEntity> Query();
        IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject);
        IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query);
        Task<TEntity> FindAsync(params object[] keyValues);
        Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);
        Task<bool> DeleteAsync(params object[] keyValues);
        Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues);
        IQueryable<TEntity> Queryable();
    }
}