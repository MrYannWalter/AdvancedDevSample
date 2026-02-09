using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.Entities
{
    /// <summary>
    /// Représente un client qui peut passer des commandes.
    /// </summary>
    public class Customer
    {
        public Guid Id { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }

        public Customer(Guid id, string firstName, string lastName, string email, bool isActive = true)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            FirstName = firstName ?? throw new DomainException("Le prénom du client est obligatoire.");
            LastName = lastName ?? throw new DomainException("Le nom du client est obligatoire.");
            Email = email ?? throw new DomainException("L'email du client est obligatoire.");
            IsActive = isActive;
        }

        public Customer()
        {
            IsActive = true;
        }

        public void UpdateInfo(string firstName, string lastName, string email)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new DomainException("Le prénom du client est obligatoire.");
            if (string.IsNullOrWhiteSpace(lastName))
                throw new DomainException("Le nom du client est obligatoire.");
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("L'email du client est obligatoire.");
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
