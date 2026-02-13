using AdvancedDevSample.Application.DTOs.Product.ProductRequests;
using AdvancedDevSample.Application.Exceptions;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedDevSample.Application.Services
{
    public class ProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository; 
        }

        public Product CreateProduct(CreateProductRequest request)
        {
            var product = new Product(request.Name, request.Description, request.Price);
            _repository.Add(product);
            return product;
        }

        public void ChangeProductPrice(Guid productId, ChangePriceRequest request)
        {
            var product = GetProduct(productId);
            product.ChangePrice(request.NewPrice);
            _repository.Save(product);
        }
        public Product GetProduct(Guid productId)
        {
            return _repository.GetById(productId)
                ?? throw new ApplicationServiceException("Product not found", System.Net.HttpStatusCode.NotFound);
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _repository.ListAll();
        }

        public void UpdateProduct(Guid productId, UpdateProductRequest request)
        {
            var product = GetProduct(productId);
            product.UpdateInfo(request.Name, request.Description);
            _repository.Save(product);
        }

        public void DeleteProduct(Guid productId)
        {
            _repository.Remove(productId);
        }

        public void ActivateProduct(Guid productId)
        {
            var product = GetProduct(productId);
            product.Activate();
            _repository.Save(product);
        }

        public void DeactivateProduct(Guid productId)
        {
            var product = GetProduct(productId);
            product.Deactivate();
            _repository.Save(product);
        }

    }
}
