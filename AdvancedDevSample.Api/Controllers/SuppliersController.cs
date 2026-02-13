using AdvancedDevSample.Application.DTOs.Supplier.SupplierRequests;
using AdvancedDevSample.Application.DTOs.Supplier.SupplierResponses;
using AdvancedDevSample.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedDevSample.Api.Controllers
{
    /// <summary>
    /// Gestion des fournisseurs.
    /// </summary>
    [ApiController]
    [Route("api/suppliers")]
    public class SuppliersController : ControllerBase
    {
        private readonly SupplierService _supplierService;

        public SuppliersController(SupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        /// <summary>
        /// Récupère la liste de tous les fournisseurs.
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var suppliers = _supplierService.GetAllSuppliers();
            var response = suppliers.Select(SupplierResponse.FromSupplier);
            return Ok(response);
        }

        /// <summary>
        /// Récupère un fournisseur par son identifiant.
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var supplier = _supplierService.GetSupplier(id);
            return Ok(SupplierResponse.FromSupplier(supplier));
        }

        /// <summary>
        /// Crée un nouveau fournisseur.
        /// </summary>
        [HttpPost]
        public IActionResult Create([FromBody] CreateSupplierRequest request)
        {
            var supplier = _supplierService.CreateSupplier(request);
            var response = SupplierResponse.FromSupplier(supplier);
            return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, response);
        }

        /// <summary>
        /// Met à jour les informations d'un fournisseur.
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] UpdateSupplierRequest request)
        {
            _supplierService.UpdateSupplier(id, request);
            return NoContent();
        }

        /// <summary>
        /// Supprime un fournisseur.
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _supplierService.DeleteSupplier(id);
            return NoContent();
        }

        /// <summary>
        /// Active un fournisseur.
        /// </summary>
        [HttpPut("{id}/activate")]
        public IActionResult Activate(Guid id)
        {
            _supplierService.ActivateSupplier(id);
            return NoContent();
        }

        /// <summary>
        /// Désactive un fournisseur.
        /// </summary>
        [HttpPut("{id}/deactivate")]
        public IActionResult Deactivate(Guid id)
        {
            _supplierService.DeactivateSupplier(id);
            return NoContent();
        }
    }
}
