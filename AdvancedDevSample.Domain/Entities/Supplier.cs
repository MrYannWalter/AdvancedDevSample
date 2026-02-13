using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.Entities
{
    /// <summary>
    /// Représente un fournisseur de produits.
    /// </summary>
    public class Supplier
    {
        public Guid Id { get; private set; }
        public string CompanyName { get; private set; } = string.Empty;
        public string ContactEmail { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }

        public Supplier(string companyName, string contactEmail, string phone)
        {
            Id = Guid.NewGuid();
            CompanyName = companyName ?? throw new DomainException("Le nom de la société est obligatoire.");
            ContactEmail = contactEmail ?? throw new DomainException("L'email de contact est obligatoire.");
            Phone = phone ?? string.Empty;
            IsActive = true;
        }

        public Supplier()
        {
            IsActive = true;
        }

        public void UpdateInfo(string companyName, string contactEmail, string phone)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                throw new DomainException("Le nom de la société est obligatoire.");
            if (string.IsNullOrWhiteSpace(contactEmail))
                throw new DomainException("L'email de contact est obligatoire.");
            CompanyName = companyName;
            ContactEmail = contactEmail;
            Phone = phone ?? string.Empty;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
