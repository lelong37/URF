using System;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Northwind.Entities.Models;
using Repository.Pattern.Ef6;
using Repository.Pattern.Repositories;
using Repository.Pattern.UnitOfWork;
using TrackableEntities;
using System.Threading.Tasks;

namespace Northwind.Test.IntegrationTests
{
    [TestClass]
    public class ProductRepositoryTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Utility.CreateSeededTestDatabase();
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void InsertProducts()
        {
            using (var context = new NorthwindContext())
            {
                IUnitOfWorkAsync unitOfWork = new UnitOfWork(context);
                IRepositoryAsync<Product> productRepository = new Repository<Product>(context, unitOfWork);

                var newProducts = new[]
                {
                    new Product {ProductName = "One", Discontinued = false, TrackingState = TrackingState.Added},
                    new Product
                    {
                        ProductName = "12345678901234567890123456789012345678901234567890",
                        Discontinued = true,
                        TrackingState = TrackingState.Added
                    },
                    new Product {ProductName = "Three", Discontinued = true, TrackingState = TrackingState.Added},
                    new Product {ProductName = "Four", Discontinued = true, TrackingState = TrackingState.Added},
                    new Product {ProductName = "Five", Discontinued = true, TrackingState = TrackingState.Added}
                };

                foreach (var product in newProducts)
                {
                    try
                    {
                        productRepository.Insert(product);
                        unitOfWork.SaveChanges();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var sb = new StringBuilder();

                        foreach (var failure in ex.EntityValidationErrors)
                        {
                            sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());

                            foreach (var error in failure.ValidationErrors)
                            {
                                sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                                sb.AppendLine();
                            }
                        }

                        Debug.WriteLine(sb.ToString());
                        TestContext.WriteLine(sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        TestContext.WriteLine(ex.Message);
                    }
                }

                var insertedProduct = productRepository.Query(x => x.ProductName == "One").Select().FirstOrDefault();
                Assert.IsTrue(insertedProduct?.ProductName == "One");
            }
        }

        [TestMethod]
        public async Task DeleteProductsAsync()
        {
            using (var context = new NorthwindContext())
            {
                IUnitOfWorkAsync unitOfWork = new UnitOfWork(context);
                IRepositoryAsync<Product> productRepository = new Repository<Product>(context, unitOfWork);

                var product = new Product
                {
                    ProductName = "One",
                    Discontinued = false,
                    TrackingState = TrackingState.Added
                };

                productRepository.Insert(product);
                await unitOfWork.SaveChangesAsync();

                await productRepository.DeleteAsync(product.ProductID);
                await unitOfWork.SaveChangesAsync();

                var deletedProduct = unitOfWork.Repository<Product>().Find(product.ProductID);

                Assert.IsNull(deletedProduct);
            }
        }
    }
}