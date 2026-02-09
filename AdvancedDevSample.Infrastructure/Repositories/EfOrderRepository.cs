using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Repositories
{
    public class EfOrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public EfOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Order order)
        {
            try
            {
                _context.Orders.Add(order);
                _context.SaveChanges();
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
                return _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefault(o => o.Id == id);
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
                return _context.Orders
                    .Include(o => o.Items)
                    .ToList();
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
                return _context.Orders
                    .Include(o => o.Items)
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
                _context.Orders.Update(order);
                _context.SaveChanges();
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
                var order = _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefault(o => o.Id == id);
                if (order != null)
                {
                    _context.Orders.Remove(order);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la suppression de la commande.", ex);
            }
        }
    }
}
