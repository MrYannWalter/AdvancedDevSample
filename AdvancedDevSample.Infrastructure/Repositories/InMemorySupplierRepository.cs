using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;

namespace AdvancedDevSample.Infrastructure.Repositories
{
    public class InMemorySupplierRepository : ISupplierRepository
    {
        private readonly InMemoryDataStore _store;

        public InMemorySupplierRepository(InMemoryDataStore store)
        {
            _store = store;
        }

        public void Add(Supplier supplier)
        {
            try
            {
                _store.Suppliers[supplier.Id] = supplier;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de l'ajout du fournisseur.", ex);
            }
        }

        public Supplier? GetById(Guid id)
        {
            try
            {
                return _store.Suppliers.TryGetValue(id, out var supplier) ? supplier : null;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la recherche du fournisseur.", ex);
            }
        }

        public IEnumerable<Supplier> ListAll()
        {
            try
            {
                return _store.Suppliers.Values.ToList();
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la récupération des fournisseurs.", ex);
            }
        }

        public void Save(Supplier supplier)
        {
            try
            {
                _store.Suppliers[supplier.Id] = supplier;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la sauvegarde du fournisseur.", ex);
            }
        }

        public void Remove(Guid id)
        {
            try
            {
                _store.Suppliers.Remove(id);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la suppression du fournisseur.", ex);
            }
        }
    }
}
