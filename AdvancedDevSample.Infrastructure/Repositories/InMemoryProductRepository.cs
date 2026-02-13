using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;

namespace AdvancedDevSample.Infrastructure.Repositories
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly InMemoryDataStore _store;

        public InMemoryProductRepository(InMemoryDataStore store)
        {
            _store = store;
        }

        public void Add(Product product)
        {
            try
            {
                _store.Products[product.Id] = product;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de l'ajout du produit.", ex);
            }
        }

        public Product? GetById(Guid id)
        {
            try
            {
                return _store.Products.TryGetValue(id, out var product) ? product : null;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la recherche du produit.", ex);
            }
        }

        public IEnumerable<Product> ListAll()
        {
            try
            {
                return _store.Products.Values.ToList();
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la récupération des produits.", ex);
            }
        }

        public void Save(Product product)
        {
            try
            {
                _store.Products[product.Id] = product;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la sauvegarde du produit.", ex);
            }
        }

        public void Remove(Guid id)
        {
            try
            {
                _store.Products.Remove(id);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la suppression du produit.", ex);
            }
        }
    }
}
