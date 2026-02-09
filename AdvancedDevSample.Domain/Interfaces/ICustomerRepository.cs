using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Domain.Interfaces
{
    public interface ICustomerRepository
    {
        void Add(Customer customer);
        void Save(Customer customer);
        Customer? GetById(Guid customerId);
        IEnumerable<Customer> ListAll();
        void Remove(Guid customerId);
    }
}
