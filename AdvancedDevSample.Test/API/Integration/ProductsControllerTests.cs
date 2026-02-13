using System.Net;
using System.Net.Http.Json;
using AdvancedDevSample.Application.DTOs.Product.ProductRequests;
using AdvancedDevSample.Application.DTOs.Product.ProductResponses;
using AdvancedDevSample.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AdvancedDevSample.Test.API.Integration
{
    /// <summary>
    /// Tests d'intégration du ProductsController.
    /// Utilise WebApplicationFactory avec la vraie configuration DI (InMemoryDataStore Singleton).
    /// Chaque test crée ses propres données pour éviter les collisions entre tests.
    /// </summary>
    public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public ProductsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Crée un produit via l'API et retourne sa réponse désérialisée.
        /// </summary>
        private async Task<ProductResponse> CreerProduitViaApi(string nom = "Produit Test", decimal prix = 50m)
        {
            var requete = new CreateProductRequest
            {
                Name = nom,
                Description = "Description test",
                Price = prix
            };
            var response = await _client.PostAsJsonAsync("/api/products", requete);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
        }

        #region GET

        [Fact]
        public async Task GetAll_DoitRetournerOk_AvecListeDeProduits()
        {
            // Arrange : créer un produit pour s'assurer qu'il y a au moins un résultat
            await CreerProduitViaApi();

            // Act : récupérer tous les produits
            var response = await _client.GetAsync("/api/products");

            // Assert : statut 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var produits = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
            Assert.NotNull(produits);
            Assert.NotEmpty(produits);
        }

        [Fact]
        public async Task GetById_DoitRetournerOk_QuandProduitExiste()
        {
            // Arrange : créer un produit
            var produit = await CreerProduitViaApi("Clavier");

            // Act : récupérer par identifiant
            var response = await _client.GetAsync($"/api/products/{produit.Id}");

            // Assert : statut 200, données correctes
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var resultat = await response.Content.ReadFromJsonAsync<ProductResponse>();
            Assert.Equal("Clavier", resultat!.Name);
        }

        [Fact]
        public async Task GetById_DoitRetournerNotFound_QuandProduitInexistant()
        {
            // Act : identifiant inconnu
            var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

            // Assert : statut 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST

        [Fact]
        public async Task Create_DoitRetournerCreated_QuandRequeteValide()
        {
            // Arrange : requête valide
            var requete = new CreateProductRequest
            {
                Name = "Souris",
                Description = "Souris sans fil",
                Price = 29.99m
            };

            // Act : créer le produit
            var response = await _client.PostAsJsonAsync("/api/products", requete);

            // Assert : statut 201 Created, corps avec les données
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var produit = await response.Content.ReadFromJsonAsync<ProductResponse>();
            Assert.NotNull(produit);
            Assert.Equal("Souris", produit.Name);
            Assert.Equal(29.99m, produit.Price);
            Assert.True(produit.IsActive);
        }

        [Fact]
        public async Task Create_DoitRetournerBadRequest_QuandDonneesInvalides()
        {
            // Arrange : requête avec nom null (va lever DomainException → 400)
            var requete = new { Name = (string?)null, Description = "Desc", Price = 10m };

            // Act : tenter la création
            var response = await _client.PostAsJsonAsync("/api/products", requete);

            // Assert : statut 400 Bad Request
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT

        [Fact]
        public async Task Update_DoitRetournerNoContent_QuandProduitExiste()
        {
            // Arrange : produit existant
            var produit = await CreerProduitViaApi();
            var requete = new UpdateProductRequest { Name = "Nouveau nom", Description = "Nouvelle desc" };

            // Act : mettre à jour
            var response = await _client.PutAsJsonAsync($"/api/products/{produit.Id}", requete);

            // Assert : statut 204 No Content
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Update_DoitRetournerNotFound_QuandProduitInexistant()
        {
            // Act : identifiant inconnu
            var requete = new UpdateProductRequest { Name = "Nom", Description = "Desc" };
            var response = await _client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}", requete);

            // Assert : statut 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ChangePrice_DoitRetournerNoContent_QuandPrixValide()
        {
            // Arrange : produit existant
            var produit = await CreerProduitViaApi();
            var requete = new ChangePriceRequest { NewPrice = 99.99m };

            // Act : changer le prix
            var response = await _client.PutAsJsonAsync($"/api/products/{produit.Id}/price", requete);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task ChangePrice_DoitRetournerBadRequest_QuandProduitInactif()
        {
            // Arrange : produit désactivé
            var produit = await CreerProduitViaApi();
            await _client.PutAsync($"/api/products/{produit.Id}/deactivate", null);

            // Act : tenter de changer le prix
            var requete = new ChangePriceRequest { NewPrice = 75m };
            var response = await _client.PutAsJsonAsync($"/api/products/{produit.Id}/price", requete);

            // Assert : statut 400 (DomainException → produit inactif)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region DELETE

        [Fact]
        public async Task Delete_DoitRetournerNoContent_QuandProduitExiste()
        {
            // Arrange : produit existant
            var produit = await CreerProduitViaApi();

            // Act : supprimer
            var response = await _client.DeleteAsync($"/api/products/{produit.Id}");

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion

        #region Activation / Désactivation

        [Fact]
        public async Task Activate_DoitRetournerNoContent()
        {
            // Arrange : produit désactivé
            var produit = await CreerProduitViaApi();
            await _client.PutAsync($"/api/products/{produit.Id}/deactivate", null);

            // Act : réactiver
            var response = await _client.PutAsync($"/api/products/{produit.Id}/activate", null);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Deactivate_DoitRetournerNoContent()
        {
            // Arrange : produit actif
            var produit = await CreerProduitViaApi();

            // Act : désactiver
            var response = await _client.PutAsync($"/api/products/{produit.Id}/deactivate", null);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion
    }
}
