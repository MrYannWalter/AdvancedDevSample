using AdvancedDevSample.Application.DTOs.Order.OrderRequests;
using AdvancedDevSample.Application.Exceptions;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using System.Net;

namespace AdvancedDevSample.Application.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
        }

        public IEnumerable<Order> GetAllOrders()
        {
            return _orderRepository.ListAll();
        }

        public Order GetOrder(Guid orderId)
        {
            return _orderRepository.GetById(orderId)
                ?? throw new ApplicationServiceException("Commande introuvable.", HttpStatusCode.NotFound);
        }

        public IEnumerable<Order> GetOrdersByCustomer(Guid customerId)
        {
            return _orderRepository.GetByCustomerId(customerId);
        }

        public Order CreateOrder(CreateOrderRequest request)
        {
            var customer = _customerRepository.GetById(request.CustomerId)
                ?? throw new ApplicationServiceException("Client introuvable.", HttpStatusCode.NotFound);

            var order = new Order(customer.Id);
            _orderRepository.Add(order);
            return order;
        }

        public void AddItemToOrder(Guid orderId, AddOrderItemRequest request)
        {
            var order = GetOrder(orderId);
            var product = _productRepository.GetById(request.ProductId)
                ?? throw new ApplicationServiceException("Produit introuvable.", HttpStatusCode.NotFound);

            order.AddItem(product.Id, request.Quantity, product.Price);
            _orderRepository.Save(order);
        }

        public void RemoveItemFromOrder(Guid orderId, Guid itemId)
        {
            var order = GetOrder(orderId);
            order.RemoveItem(itemId);
            _orderRepository.Save(order);
        }

        public void ConfirmOrder(Guid orderId)
        {
            var order = GetOrder(orderId);
            order.Confirm();
            _orderRepository.Save(order);
        }

        public void ShipOrder(Guid orderId)
        {
            var order = GetOrder(orderId);
            order.Ship();
            _orderRepository.Save(order);
        }

        public void DeliverOrder(Guid orderId)
        {
            var order = GetOrder(orderId);
            order.Deliver();
            _orderRepository.Save(order);
        }

        public void CancelOrder(Guid orderId)
        {
            var order = GetOrder(orderId);
            order.Cancel();
            _orderRepository.Save(order);
        }

        public void DeleteOrder(Guid orderId)
        {
            var order = GetOrder(orderId);
            _orderRepository.Remove(orderId);
        }
    }
}
