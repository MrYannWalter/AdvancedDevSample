using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Domain.Interfaces
{
    public interface IOrderRepository
    {
        void Add(Order order);
        void Save(Order order);
        Order? GetById(Guid orderId);
        IEnumerable<Order> ListAll();
        IEnumerable<Order> GetByCustomerId(Guid customerId);
        void Remove(Guid orderId);
    }
}
