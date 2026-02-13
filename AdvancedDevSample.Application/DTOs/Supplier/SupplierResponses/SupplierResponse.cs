namespace AdvancedDevSample.Application.DTOs.Supplier.SupplierResponses
{
    using AdvancedDevSample.Domain.Entities;

    public class SupplierResponse
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public static SupplierResponse FromSupplier(Supplier supplier)
        {
            return new SupplierResponse
            {
                Id = supplier.Id,
                CompanyName = supplier.CompanyName,
                ContactEmail = supplier.ContactEmail,
                Phone = supplier.Phone,
                IsActive = supplier.IsActive
            };
        }
    }
}
