namespace AdvancedDevSample.Application.DTOs.Order.OrderResponses
{
    public class OrderResponse
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemResponse> Items { get; set; } = new();

        public static OrderResponse FromOrder(Domain.Entities.Order order)
        {
            return new OrderResponse
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                Items = order.Items.Select(OrderItemResponse.FromOrderItem).ToList()
            };
        }
    }
}
