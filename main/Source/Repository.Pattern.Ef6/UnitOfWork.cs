using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using CommonServiceLocator;
using Repository.Pattern.Repositories;
using Repository.Pattern.UnitOfWork;
using TrackableEntities;

namespace Repository.Pattern.Ef6
{
    public class UnitOfWork : IUnitOfWorkAsync
    {
        private readonly DbContext _context;
        protected DbTransaction Transaction;
        protected Dictionary<string, dynamic> Repositories;

        public UnitOfWork(DbContext context)
        {
            _context = context;
            Repositories = new Dictionary<string, dynamic>();
        }

        public virtual IRepository<TEntity> Repository<TEntity>() where TEntity : class, ITrackable
        {
            if (ServiceLocator.IsLocationProviderSet)
            {
                return ServiceLocator.Current.GetInstance<IRepository<TEntity>>();
            }

            return RepositoryAsync<TEntity>();
        }

        public int? CommandTimeout
        {
            get => _context.Database.CommandTimeout;
            set => _context.Database.CommandTimeout = value;
        }

        public virtual int SaveChanges() => _context.SaveChanges();

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public virtual IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : class, ITrackable
        {
            if (ServiceLocator.IsLocationProviderSet)
            {
                return ServiceLocator.Current.GetInstance<IRepositoryAsync<TEntity>>();
            }

            if (Repositories == null)
            {
                Repositories = new Dictionary<string, dynamic>();
            }

            var type = typeof(TEntity).Name;

            if (Repositories.ContainsKey(type))
            {
                return (IRepositoryAsync<TEntity>)Repositories[type];
            }

            var repositoryType = typeof(Repository<>);

            Repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context, this));

            return Repositories[type];
        }

        public virtual int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return _context.Database.ExecuteSqlCommand(sql, parameters);
        }

        public virtual async Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlCommandAsync(sql, parameters);
        }

        public virtual async Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken cancellationToken, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlCommandAsync(sql, cancellationToken, parameters);
        }

        public virtual void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            var objectContext = ((IObjectContextAdapter) _context).ObjectContext;
            if (objectContext.Connection.State != ConnectionState.Open)
            {
                objectContext.Connection.Open();
            }
            Transaction = objectContext.Connection.BeginTransaction(isolationLevel);
        }

        public virtual bool Commit()
        {
            Transaction.Commit();
            return true;
        }

        public virtual void Rollback()
        {
            Transaction.Rollback();
        }

        public virtual async Task ExecuteDataReaderAsync(string sql, object parameters, Action<DbDataReader> action)
        {
            if (_context.Database.Connection.State != ConnectionState.Open)
                await _context.Database.Connection.OpenAsync();
            var command = _context.Database.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            SetParameters(command, parameters);
            using (var dr = await command.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                    action.Invoke(dr);
            }

        }
        public  void SetParameters(DbCommand cmd, object parameters)
        {
            cmd.Parameters.Clear();
            if (parameters == null)
                return;
            if (parameters is IList)
            {
                var listed = (IList)parameters;
                for (var i = 0; i < listed.Count; i++)
                {
                    AddParameter(cmd, string.Format("@{0}", i), listed[i]);
                }
            }
            else
            {
                var t = parameters.GetType();
                var parameterInfos = t.GetProperties();
                foreach (var pi in parameterInfos)
                {
                    AddParameter(cmd, pi.Name, pi.GetValue(parameters, null));
                }
            }
        }

        private  void AddParameter(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}