using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Repositories
{
    public class EfProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public EfProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Product product)
        {
            try
            {
                _context.Products.Add(product);
                _context.SaveChanges();
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
                return _context.Products.FirstOrDefault(p => p.Id == id);
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
                return _context.Products.ToList();
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
                _context.Products.Update(product);
                _context.SaveChanges();
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
                var product = _context.Products.FirstOrDefault(p => p.Id == id);
                if (product != null)
                {
                    _context.Products.Remove(product);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Erreur lors de la suppression du produit.", ex);
            }
        }
    }
}
