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

        public Product(string name, string description, decimal price)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new DomainException("Le nom du produit est obligatoire.");
            Description = description ?? throw new DomainException("La description du produit est obligatoire."); ;
            Price = price > 0 ? price : throw new DomainException("Le prix doit être strictement positif.");
            IsActive = true;
        }

        // Constructeur requis par certains ORMs
        public Product()
        {
            IsActive = true;
        }

        public void ChangePrice(decimal newPrice)
        {
            CheckIfActive();
            if (newPrice <= 0)
                throw new DomainException("Le prix doit être strictement positif.");
            Price = newPrice;
        }

        public void UpdateInfo(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Le nom du produit est obligatoire.");
            Name = name;
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("La description du produit est obligatoire.");
            Description = description;
        }

        public void ApplyDiscount(decimal percentage)
        {
            CheckIfActive();
            if (percentage <= 0 || percentage >= 100)
                throw new DomainException("Le pourcentage de réduction doit être compris entre 0 et 100.");

            var newPrice = Price * (1 - percentage / 100);
            ChangePrice(newPrice);
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
        public void CheckIfActive()
        {
            if (!IsActive)
                throw new DomainException("Le produit est inactif.");
        }
    }
}
