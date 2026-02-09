using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Test.API.Integration
{
    public class InMemoryProductRepositoryAsync : IProductRepository
    {
        private readonly Dictionary<Guid, Product> _store = new();

       // public Task<Product?> GetById(Guid id)
         //   => Task.FromResult(_store.TryGetValue(id, out var product) ? product : null);

        //public Task save(Product product)
        //{
          //  _store[product.Id] = product;
            //return Task.CompletedTask;
        //}

        //Helper pour initialiser le test
        public void Seed(Product product)
            => _store[product.Id] = product;

        public void Save(Product product)
        {
            throw new NotImplementedException();
        }

        public Product GetById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
