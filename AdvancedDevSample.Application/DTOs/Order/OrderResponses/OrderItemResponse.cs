using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Application.DTOs.Order.OrderResponses
{
    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }

        public static OrderItemResponse FromOrderItem(OrderItem item)
        {
            return new OrderItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Total = item.GetTotal()
            };
        }
    }
}
