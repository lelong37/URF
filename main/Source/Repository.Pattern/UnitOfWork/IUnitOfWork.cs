using System;
using System.Data;
using Repository.Pattern.Repositories;
using TrackableEntities;

namespace Repository.Pattern.UnitOfWork
{
    public interface IUnitOfWork
    {
        int SaveChanges();
        int ExecuteSqlCommand(string sql, params object[] parameters);
        IRepository<TEntity> Repository<TEntity>() where TEntity : class, ITrackable;
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        bool Commit();
        void Rollback();
    }
}