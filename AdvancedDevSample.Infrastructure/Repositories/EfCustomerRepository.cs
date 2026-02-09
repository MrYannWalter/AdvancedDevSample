using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;

namespace AdvancedDevSample.Infrastructure.Repositories
{
    public class EfCustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public EfCustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Customer customer)
        {
            try
            {
                _context.Customers.Add(customer);
                _context.SaveChanges();
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
                return _context.Customers.FirstOrDefault(c => c.Id == id);
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
                return _context.Customers.ToList();
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
                _context.Customers.Update(customer);
                _context.SaveChanges();
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
                var customer = _context.Customers.FirstOrDefault(c => c.Id == id);
                if (customer != null)
                {
                    _context.Customers.Remove(customer);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la suppression du client.", ex);
            }
        }
    }
}
