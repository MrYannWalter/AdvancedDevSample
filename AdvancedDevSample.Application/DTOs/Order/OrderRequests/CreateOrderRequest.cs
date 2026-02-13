using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs.Order.OrderRequests
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Le client est obligatoire.")]
        public Guid CustomerId { get; set; }
    }
}
