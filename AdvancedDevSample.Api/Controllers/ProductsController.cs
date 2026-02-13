using AdvancedDevSample.Application.DTOs.Product.ProductRequests;
using AdvancedDevSample.Application.DTOs.Product.ProductResponses;
using AdvancedDevSample.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedDevSample.Api.Controllers
{
    /// <summary>
    /// Gestion du catalogue de produits.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Récupère la liste de tous les produits.
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _productService.GetAllProducts();
            var response = products.Select(ProductResponse.FromProduct);
            return Ok(response);
        }

        /// <summary>
        /// Récupère un produit par son identifiant.
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var product = _productService.GetProduct(id);
            return Ok(ProductResponse.FromProduct(product));
        }

        /// <summary>
        /// Crée un nouveau produit dans le catalogue.
        /// </summary>
        [HttpPost]
        public IActionResult Create([FromBody] CreateProductRequest request)
        {
            var product = _productService.CreateProduct(request);
            var response = ProductResponse.FromProduct(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
        }

        /// <summary>
        /// Met à jour les informations d'un produit.
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            _productService.UpdateProduct(id, request);
            return NoContent();
        }

        /// <summary>
        /// Modifie le prix d'un produit.
        /// </summary>
        [HttpPut("{id}/price")]
        public IActionResult ChangePrice(Guid id, [FromBody] ChangePriceRequest request)
        {
            _productService.ChangeProductPrice(id, request);
            return NoContent();
        }

        /// <summary>
        /// Supprime un produit du catalogue.
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _productService.DeleteProduct(id);
            return NoContent();
        }

        /// <summary>
        /// Active un produit.
        /// </summary>
        [HttpPut("{id}/activate")]
        public IActionResult Activate(Guid id)
        {
            _productService.ActivateProduct(id);
            return NoContent();
        }

        /// <summary>
        /// Désactive un produit.
        /// </summary>
        [HttpPut("{id}/deactivate")]
        public IActionResult Deactivate(Guid id)
        {
            _productService.DeactivateProduct(id);
            return NoContent();
        }
    }
}
