using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedDevSample.Api.Controllers
{
    /// <summary>
    /// Gestion des commandes.
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Récupère la liste de toutes les commandes.
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var orders = _orderService.GetAllOrders();
            var response = orders.Select(OrderResponse.FromEntity);
            return Ok(response);
        }

        /// <summary>
        /// Récupère une commande par son identifiant.
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var order = _orderService.GetOrder(id);
            return Ok(OrderResponse.FromEntity(order));
        }

        /// <summary>
        /// Récupère les commandes d'un client.
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public IActionResult GetByCustomer(Guid customerId)
        {
            var orders = _orderService.GetOrdersByCustomer(customerId);
            var response = orders.Select(OrderResponse.FromEntity);
            return Ok(response);
        }

        /// <summary>
        /// Crée une nouvelle commande pour un client.
        /// </summary>
        [HttpPost]
        public IActionResult Create([FromBody] CreateOrderRequest request)
        {
            var order = _orderService.CreateOrder(request);
            var response = OrderResponse.FromEntity(order);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, response);
        }

        /// <summary>
        /// Ajoute un article à une commande.
        /// </summary>
        [HttpPost("{id}/items")]
        public IActionResult AddItem(Guid id, [FromBody] AddOrderItemRequest request)
        {
            _orderService.AddItemToOrder(id, request);
            return NoContent();
        }

        /// <summary>
        /// Retire un article d'une commande.
        /// </summary>
        [HttpDelete("{id}/items/{itemId}")]
        public IActionResult RemoveItem(Guid id, Guid itemId)
        {
            _orderService.RemoveItemFromOrder(id, itemId);
            return NoContent();
        }

        /// <summary>
        /// Confirme une commande en attente.
        /// </summary>
        [HttpPut("{id}/confirm")]
        public IActionResult Confirm(Guid id)
        {
            _orderService.ConfirmOrder(id);
            return NoContent();
        }

        /// <summary>
        /// Marque une commande comme expédiée.
        /// </summary>
        [HttpPut("{id}/ship")]
        public IActionResult Ship(Guid id)
        {
            _orderService.ShipOrder(id);
            return NoContent();
        }

        /// <summary>
        /// Marque une commande comme livrée.
        /// </summary>
        [HttpPut("{id}/deliver")]
        public IActionResult Deliver(Guid id)
        {
            _orderService.DeliverOrder(id);
            return NoContent();
        }

        /// <summary>
        /// Annule une commande.
        /// </summary>
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(Guid id)
        {
            _orderService.CancelOrder(id);
            return NoContent();
        }

        /// <summary>
        /// Supprime une commande.
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _orderService.DeleteOrder(id);
            return NoContent();
        }
    }
}
