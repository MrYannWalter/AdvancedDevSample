namespace AdvancedDevSample.Domain.Enums
{
    /// <summary>
    /// Représente les différents états possibles d'une commande.
    /// </summary>
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }
}
