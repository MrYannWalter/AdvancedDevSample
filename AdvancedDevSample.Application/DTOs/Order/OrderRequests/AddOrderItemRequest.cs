using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs.Order.OrderRequests
{
    public class AddOrderItemRequest
    {
        [Required(ErrorMessage = "Le produit est obligatoire.")]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être supérieure à zéro.")]
        public int Quantity { get; set; }
    }
}
