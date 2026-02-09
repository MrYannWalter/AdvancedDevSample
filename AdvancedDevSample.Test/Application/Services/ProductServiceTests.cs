using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Test.Application.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedDevSample.Test.Application.Services
{
    public class ProductServiceTests
    {
        [Fact]
        public void ChangeProductPrice_Should_Save_Product_When_Price_Is_Valid()
        {
            //Arrange
            var product = new Product();
            product.ChangePrice(10); //état initial valide

            var repo = new FakeProductRepository(product);
            var service = new ProductService(repo);

            //Act
            var request = new ChangePriceRequest { NewPrice = 20 };
            service.ChangeProductPrice(product.Id, request);

            //Assert
            Assert.Equal(20, product.Price);
            Assert.True(repo.WasSaved);
        }
    }
}
