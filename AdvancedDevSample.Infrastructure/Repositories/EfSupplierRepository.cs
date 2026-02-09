using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;

namespace AdvancedDevSample.Infrastructure.Repositories
{
    public class EfSupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;

        public EfSupplierRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Supplier supplier)
        {
            try
            {
                _context.Suppliers.Add(supplier);
                _context.SaveChanges();
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
                return _context.Suppliers.FirstOrDefault(s => s.Id == id);
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
                return _context.Suppliers.ToList();
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
                _context.Suppliers.Update(supplier);
                _context.SaveChanges();
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
                var supplier = _context.Suppliers.FirstOrDefault(s => s.Id == id);
                if (supplier != null)
                {
                    _context.Suppliers.Remove(supplier);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la suppression du fournisseur.", ex);
            }
        }
    }
}
