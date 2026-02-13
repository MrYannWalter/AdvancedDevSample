using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs.Supplier.SupplierRequests
{
    public class CreateSupplierRequest
    {
        [Required(ErrorMessage = "Le nom de la société est obligatoire.")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email de contact est obligatoire.")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide.")]
        public string ContactEmail { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }
}
