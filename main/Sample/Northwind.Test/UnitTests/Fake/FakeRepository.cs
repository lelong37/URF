using System.Data.Entity;
using Repository.Pattern.Ef6;
using Repository.Pattern.UnitOfWork;
using TrackableEntities;

namespace Northwind.Test.UnitTests.Fake
{
    public class FakeRepository<TEntity> : Repository<TEntity> where TEntity : class, ITrackable
    {
        public FakeRepository(DbContext context, IUnitOfWorkAsync unitOfWork)
            : base(context, unitOfWork)
        {
        }

        public override void Insert(TEntity entity, bool traverseGraph = true)
        {
            entity.TrackingState = TrackingState.Added;
            Set.Attach(entity);
        }

        public override void Update(TEntity entity, bool traverseGraph = true)
        {
            entity.TrackingState = TrackingState.Modified;
            Set.Attach(entity);
        }

        public override void Delete(TEntity entity)
        {
            entity.TrackingState = TrackingState.Deleted;
            Set.Attach(entity);
        }
    }
}