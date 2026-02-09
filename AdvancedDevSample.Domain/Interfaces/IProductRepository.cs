using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Domain.Interfaces
{
    public interface IProductRepository
    {
        void Add(Product product);
        void Save(Product product);
        Product? GetById(Guid productId);
        IEnumerable<Product> ListAll();
        void Remove(Guid productId);
    }
}
