using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedDevSample.Api.Controllers
{
    /// <summary>
    /// Gestion des clients.
    /// </summary>
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomersController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Récupère la liste de tous les clients.
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var customers = _customerService.GetAllCustomers();
            var response = customers.Select(CustomerResponse.FromEntity);
            return Ok(response);
        }

        /// <summary>
        /// Récupère un client par son identifiant.
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var customer = _customerService.GetCustomer(id);
            return Ok(CustomerResponse.FromEntity(customer));
        }

        /// <summary>
        /// Crée un nouveau client.
        /// </summary>
        [HttpPost]
        public IActionResult Create([FromBody] CreateCustomerRequest request)
        {
            var customer = _customerService.CreateCustomer(request);
            var response = CustomerResponse.FromEntity(customer);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, response);
        }

        /// <summary>
        /// Met à jour les informations d'un client.
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] UpdateCustomerRequest request)
        {
            _customerService.UpdateCustomer(id, request);
            return NoContent();
        }

        /// <summary>
        /// Supprime un client.
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _customerService.DeleteCustomer(id);
            return NoContent();
        }

        /// <summary>
        /// Active un client.
        /// </summary>
        [HttpPut("{id}/activate")]
        public IActionResult Activate(Guid id)
        {
            _customerService.ActivateCustomer(id);
            return NoContent();
        }

        /// <summary>
        /// Désactive un client.
        /// </summary>
        [HttpPut("{id}/deactivate")]
        public IActionResult Deactivate(Guid id)
        {
            _customerService.DeactivateCustomer(id);
            return NoContent();
        }
    }
}
