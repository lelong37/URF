using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TrackableEntities;

namespace Repository.Pattern.Repositories
{
    public interface IRepository<TEntity> where TEntity : class, ITrackable
    {
        TEntity Find(params object[] keyValues);
        IQueryable<TEntity> SelectQuery(string query, params object[] parameters);
        void Insert(TEntity entity, bool traverseGraph = true);
        void ApplyChanges(TEntity entity);
        void InsertRange(IEnumerable<TEntity> entities, bool traverseGraph = true);
        [Obsolete("InsertOrUpdateGraph has been deprecated.  Instead set TrackingState to Added or Modified and call ApplyChanges.")]
        void InsertOrUpdateGraph(TEntity entity);
        [Obsolete("InsertGraphRange has been deprecated. Instead call Insert to set TrackingState on enttites in a graph.")]
        void InsertGraphRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity, bool traverseGraph = true);
        void Delete(params object[] keyValues);
        void Delete(TEntity entity);
        IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject);
        IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query);
        IQueryFluent<TEntity> Query();
        IQueryable<TEntity> Queryable();
        IRepository<T> GetRepository<T>() where T : class, ITrackable;
    }
}