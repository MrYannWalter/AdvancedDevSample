using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;

namespace AdvancedDevSample.Infrastructure.Repositories
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly InMemoryDataStore _store;

        public InMemoryOrderRepository(InMemoryDataStore store)
        {
            _store = store;
        }

        public void Add(Order order)
        {
            try
            {
                _store.Orders[order.Id] = order;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de l'ajout de la commande.", ex);
            }
        }

        public Order? GetById(Guid id)
        {
            try
            {
                return _store.Orders.TryGetValue(id, out var order) ? order : null;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la recherche de la commande.", ex);
            }
        }

        public IEnumerable<Order> ListAll()
        {
            try
            {
                return _store.Orders.Values.ToList();
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la récupération des commandes.", ex);
            }
        }

        public IEnumerable<Order> GetByCustomerId(Guid customerId)
        {
            try
            {
                return _store.Orders.Values
                    .Where(o => o.CustomerId == customerId)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la recherche des commandes du client.", ex);
            }
        }

        public void Save(Order order)
        {
            try
            {
                _store.Orders[order.Id] = order;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la sauvegarde de la commande.", ex);
            }
        }

        public void Remove(Guid id)
        {
            try
            {
                _store.Orders.Remove(id);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la suppression de la commande.", ex);
            }
        }
    }
}
