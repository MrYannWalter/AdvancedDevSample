using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.Entities
{
    /// <summary>
    /// Représente une commande passée par un client.
    /// </summary>
    public class Order
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public OrderStatus Status { get; private set; }

        private readonly List<OrderItem> _items = new();
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        public Order(Guid customerId)
        {
            Id = Guid.NewGuid();

            if (customerId == Guid.Empty)
                throw new DomainException("Le client est obligatoire pour une commande.");

            CustomerId = customerId;
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Pending;
        }

        public Order()
        {
            _items = new List<OrderItem>();
        }

        public void AddItem(Guid productId, int quantity, decimal unitPrice)
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("Impossible d'ajouter un article à une commande qui n'est pas en attente.");

            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
                throw new DomainException("Ce produit est déjà dans la commande. Modifiez la quantité à la place.");

            var item = new OrderItem(Id, productId, quantity, unitPrice);
            _items.Add(item);
        }

        public void RemoveItem(Guid orderItemId)
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("Impossible de retirer un article d'une commande qui n'est pas en attente.");

            var item = _items.FirstOrDefault(i => i.Id == orderItemId)
                ?? throw new DomainException("Article introuvable dans la commande.");

            _items.Remove(item);
        }

        public decimal CalculateTotal()
        {
            return _items.Sum(i => i.GetTotal());
        }

        public void Confirm()
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("Seule une commande en attente peut être confirmée.");
            if (!_items.Any())
                throw new DomainException("Impossible de confirmer une commande sans articles.");
            Status = OrderStatus.Confirmed;
        }

        public void Ship()
        {
            if (Status != OrderStatus.Confirmed)
                throw new DomainException("Seule une commande confirmée peut être expédiée.");
            Status = OrderStatus.Shipped;
        }

        public void Deliver()
        {
            if (Status != OrderStatus.Shipped)
                throw new DomainException("Seule une commande expédiée peut être livrée.");
            Status = OrderStatus.Delivered;
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Delivered)
                throw new DomainException("Impossible d'annuler une commande déjà livrée.");
            if (Status == OrderStatus.Cancelled)
                throw new DomainException("La commande est déjà annulée.");
            Status = OrderStatus.Cancelled;
        }
    }
}
