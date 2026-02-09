using AdvancedDevSample.Domain.ValueObjects;
using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Domain.Entities;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace AdvancedDevSample.Test.API.Integration
{
    public class ProductAsyncControllerIntergrationTest
    {
        private readonly HttpClient _client;
        private readonly InMemoryProductRepositoryAsync _repository;

        public ProductAsyncControllerIntergrationTest(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _repository = (InMemoryProductRepositoryAsync) factory.Services.GetRequiredService<InMemoryProductRepositoryAsync>();
        }
        [Fact]
        public async Task ChangePrice_Should_Return_NoContent_And_Save_Product()
        {
            //Arrange
            var product = new Product();
            product.ChangePrice(10); //état initial valide
            _repository.Seed(product);

            var request = new ChangePriceRequest { NewPrice = 20 };

            //Act
            var response = await _client.PutAsJsonAsync($"/api/productasync/{product.Id}/price", request);

            //Asser -HTTP 
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //Assert - Persistence réelle
            var updated = await _repository.GetByIdAsync(product.Id);
            Assert.Equal(20, updated! .Price);
        }
    }
}
