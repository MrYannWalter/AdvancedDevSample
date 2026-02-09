using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.Entities
{
    /// <summary>
    /// Représente une ligne de commande associée à un produit.
    /// </summary>
    public class OrderItem
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }

        public OrderItem(Guid id, Guid orderId, Guid productId, int quantity, decimal unitPrice)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            OrderId = orderId;
            ProductId = productId;

            if (quantity <= 0)
                throw new DomainException("La quantité doit être supérieure à zéro.");
            Quantity = quantity;

            if (unitPrice <= 0)
                throw new DomainException("Le prix unitaire doit être strictement positif.");
            UnitPrice = unitPrice;
        }

        public OrderItem() { }

        public decimal GetTotal() => Quantity * UnitPrice;

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new DomainException("La quantité doit être supérieure à zéro.");
            Quantity = newQuantity;
        }
    }
}
