using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.Entities
{
    /// <summary>
    /// Représente un produit vendable dans le catalogue.
    /// </summary>
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public bool IsActive { get; private set; }

        public Product(Guid id, string name, string description, decimal price, bool isActive = true)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            Name = name ?? throw new DomainException("Le nom du produit est obligatoire.");
            Description = description ?? string.Empty;
            Price = price > 0 ? price : throw new DomainException("Le prix doit être strictement positif.");
            IsActive = isActive;
        }

        // Constructeur requis par certains ORMs
        public Product()
        {
            IsActive = true;
        }

        public void ChangePrice(decimal newPrice)
        {
            if (!IsActive)
                throw new DomainException("Le produit est inactif.");
            if (newPrice <= 0)
                throw new DomainException("Le prix doit être strictement positif.");
            Price = newPrice;
        }

        public void UpdateInfo(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Le nom du produit est obligatoire.");
            Name = name;
            Description = description ?? string.Empty;
        }

        public void ApplyDiscount(decimal percentage)
        {
            if (!IsActive)
                throw new DomainException("Le produit est inactif.");
            if (percentage <= 0 || percentage >= 100)
                throw new DomainException("Le pourcentage de réduction doit être compris entre 0 et 100.");

            var newPrice = Price * (1 - percentage / 100);
            if (newPrice <= 0)
                throw new DomainException("Le prix après réduction doit rester positif.");
            Price = newPrice;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
