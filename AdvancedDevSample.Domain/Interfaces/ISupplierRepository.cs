using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Domain.Interfaces
{
    public interface ISupplierRepository
    {
        void Add(Supplier supplier);
        void Save(Supplier supplier);
        Supplier? GetById(Guid supplierId);
        IEnumerable<Supplier> ListAll();
        void Remove(Guid supplierId);
    }
}
