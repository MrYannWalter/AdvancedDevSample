using AdvancedDevSample.Application.DTOs;
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

    }
}
