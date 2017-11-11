using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LinqKit;
using Repository.Pattern.Repositories;
using Repository.Pattern.UnitOfWork;
using TrackableEntities;
using TrackableEntities.EF6;

namespace Repository.Pattern.Ef6
{
    public class Repository<TEntity> : IRepositoryAsync<TEntity> where TEntity : class, ITrackable
    {
        protected readonly DbContext Context;
        protected readonly DbSet<TEntity> Set;
        protected readonly IUnitOfWorkAsync UnitOfWork;

        public Repository(DbContext context, IUnitOfWorkAsync unitOfWork)
        {
            UnitOfWork = unitOfWork;
            Context = context;
            Set = context.Set<TEntity>();
        }

        public virtual TEntity Find(params object[] keyValues)
        {
            return Set.Find(keyValues);
        }

        public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters)
        {
            return Set.SqlQuery(query, parameters).AsQueryable();
        }

        public virtual void Insert(TEntity entity)
        {
            entity.TrackingState = TrackingState.Added;
            Context.ApplyChanges(entity);
        }

        public void ApplyChanges(TEntity entity)
        {
            Context.ApplyChanges(entity);
        }

        public virtual void InsertRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Insert(entity);
            }
        }

        [Obsolete("InsertGraphRange has been deprecated. Instead call Insert to set TrackingState on enttites in a graph.")]
        public virtual void InsertGraphRange(IEnumerable<TEntity> entities) => InsertRange(entities);

        public virtual void Update(TEntity entity)
        {
            entity.TrackingState = TrackingState.Modified;
            Context.ApplyChanges(entity);
        }

        public void Delete(params object[] keyValues)
        {
            var entity = Set.Find(keyValues);
            Delete(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            entity.TrackingState = TrackingState.Deleted;
            Context.ApplyChanges(entity);
        }

        public virtual void Delete(object id)
        {
            var entity = Set.Find(id);
            Delete(entity);
        }

        public IQueryFluent<TEntity> Query() => new QueryFluent<TEntity>(this);

        public virtual IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject) => new QueryFluent<TEntity>(this, queryObject);

        public virtual IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query) => new QueryFluent<TEntity>(this, query);

        public IQueryable<TEntity> Queryable() => Set;

        public IRepository<T> GetRepository<T>() where T : class, ITrackable => UnitOfWork.Repository<T>();

        public virtual async Task<TEntity> FindAsync(params object[] keyValues) => await Set.FindAsync(keyValues);

        public virtual async Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues) => await Set.FindAsync(cancellationToken, keyValues);

        public virtual async Task<bool> DeleteAsync(params object[] keyValues)
        {
            if (await DeleteAsync(CancellationToken.None, keyValues)) return true;
            return false;
        }

        public virtual async Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            var entity = await FindAsync(cancellationToken, keyValues);

            if (entity == null)
            {
                return false;
            }

            entity.TrackingState = TrackingState.Deleted;
            Context.ApplyChanges(entity);
            return true;
        }

        internal IQueryable<TEntity> Select(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<TEntity> query = Set;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            if (filter != null)
            {
                query = query.AsExpandable().Where(filter);
            }
            if (page != null && pageSize != null)
            {
                query = query.Skip((page.Value - 1)*pageSize.Value).Take(pageSize.Value);
            }
            return query;
        }

        public virtual async Task<IEnumerable<TEntity>> SelectQueryAsync(string query, params object[] parameters)
        {
            return await Set.SqlQuery(query, parameters).ToArrayAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> SelectQueryAsync(string query, CancellationToken cancellationToken, params object[] parameters)
        {
            return await Set.SqlQuery(query, parameters).ToArrayAsync(cancellationToken);
        }

        internal async Task<IEnumerable<TEntity>> SelectAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            return await Select(filter, orderBy, includes, page, pageSize).ToListAsync();
        }

        [Obsolete("InsertOrUpdateGraph has been deprecated.  Instead set TrackingState to Added or Modified and call ApplyChanges.")]
        public virtual void InsertOrUpdateGraph(TEntity entity)
        {
            ApplyChanges(entity);
        }
    }
}