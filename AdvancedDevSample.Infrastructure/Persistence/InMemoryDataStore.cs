using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Infrastructure.Persistence
{
    /// <summary>
    /// Stockage centralisé en mémoire utilisant des dictionnaires.
    /// Toutes les entités sont indexées par leur Guid.
    /// Les données persistent tant que l'application tourne (Singleton).
    /// </summary>
    public class InMemoryDataStore
    {
        public Dictionary<Guid, Product> Products { get; } = new();
        public Dictionary<Guid, Customer> Customers { get; } = new();
        public Dictionary<Guid, Supplier> Suppliers { get; } = new();
        public Dictionary<Guid, Order> Orders { get; } = new();
    }
}
