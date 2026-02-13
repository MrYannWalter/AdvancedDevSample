using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;

namespace AdvancedDevSample.Infrastructure.Repositories
{
    public class InMemoryCustomerRepository : ICustomerRepository
    {
        private readonly InMemoryDataStore _store;

        public InMemoryCustomerRepository(InMemoryDataStore store)
        {
            _store = store;
        }

        public void Add(Customer customer)
        {
            try
            {
                _store.Customers[customer.Id] = customer;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de l'ajout du client.", ex);
            }
        }

        public Customer? GetById(Guid id)
        {
            try
            {
                return _store.Customers.TryGetValue(id, out var customer) ? customer : null;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la recherche du client.", ex);
            }
        }

        public IEnumerable<Customer> ListAll()
        {
            try
            {
                return _store.Customers.Values.ToList();
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la récupération des clients.", ex);
            }
        }

        public void Save(Customer customer)
        {
            try
            {
                _store.Customers[customer.Id] = customer;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la sauvegarde du client.", ex);
            }
        }

        public void Remove(Guid id)
        {
            try
            {
                _store.Customers.Remove(id);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la suppression du client.", ex);
            }
        }
    }
}
