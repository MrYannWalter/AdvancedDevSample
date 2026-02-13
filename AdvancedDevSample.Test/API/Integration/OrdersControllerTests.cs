using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AdvancedDevSample.Application.DTOs.Customer.CustomerRequests;
using AdvancedDevSample.Application.DTOs.Customer.CustomerResponses;
using AdvancedDevSample.Application.DTOs.Order.OrderRequests;
using AdvancedDevSample.Application.DTOs.Order.OrderResponses;
using AdvancedDevSample.Application.DTOs.Product.ProductRequests;
using AdvancedDevSample.Application.DTOs.Product.ProductResponses;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AdvancedDevSample.Test.API.Integration
{
    /// <summary>
    /// Tests d'intégration du OrdersController.
    /// Les commandes nécessitent un client et des produits existants.
    /// Chaque test crée ses propres entités via l'API pour une isolation complète.
    /// </summary>
    public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OrdersControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        #region Helpers

        /// <summary>
        /// Crée un client via l'API et retourne son identifiant.
        /// </summary>
        private async Task<Guid> CreerClientViaApi()
        {
            var requete = new CreateCustomerRequest
            {
                FirstName = "Client",
                LastName = "Test",
                Email = $"client-{Guid.NewGuid():N}@test.com"
            };
            var response = await _client.PostAsJsonAsync("/api/customers", requete);
            response.EnsureSuccessStatusCode();
            var client = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            return client!.Id;
        }

        /// <summary>
        /// Crée un produit via l'API et retourne son identifiant.
        /// </summary>
        private async Task<Guid> CreerProduitViaApi(decimal prix = 25m)
        {
            var requete = new CreateProductRequest
            {
                Name = $"Produit-{Guid.NewGuid():N}",
                Description = "Description test",
                Price = prix
            };
            var response = await _client.PostAsJsonAsync("/api/products", requete);
            response.EnsureSuccessStatusCode();
            var produit = await response.Content.ReadFromJsonAsync<ProductResponse>();
            return produit!.Id;
        }

        /// <summary>
        /// Crée une commande via l'API et retourne sa réponse.
        /// </summary>
        private async Task<OrderResponse> CreerCommandeViaApi(Guid clientId)
        {
            var requete = new CreateOrderRequest { CustomerId = clientId };
            var response = await _client.PostAsJsonAsync("/api/orders", requete);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions))!;
        }

        /// <summary>
        /// Ajoute un article à une commande et retourne le statut HTTP.
        /// </summary>
        private async Task<HttpResponseMessage> AjouterArticleViaApi(Guid orderId, Guid produitId, int quantite = 1)
        {
            var requete = new AddOrderItemRequest { ProductId = produitId, Quantity = quantite };
            return await _client.PostAsJsonAsync($"/api/orders/{orderId}/items", requete);
        }

        #endregion

        #region GET

        [Fact]
        public async Task GetAll_DoitRetournerOk()
        {
            // Arrange : créer une commande
            var clientId = await CreerClientViaApi();
            await CreerCommandeViaApi(clientId);

            // Act : récupérer toutes les commandes
            var response = await _client.GetAsync("/api/orders");

            // Assert : statut 200
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_DoitRetournerOk_QuandCommandeExiste()
        {
            // Arrange : commande existante
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);

            // Act : récupérer par identifiant
            var response = await _client.GetAsync($"/api/orders/{commande.OrderId}");

            // Assert : statut 200, données correctes
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var resultat = await response.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
            Assert.Equal(commande.OrderId, resultat!.OrderId);
        }

        [Fact]
        public async Task GetById_DoitRetournerNotFound_QuandCommandeInexistante()
        {
            // Act : identifiant inconnu
            var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

            // Assert : statut 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByCustomer_DoitRetournerOk_AvecCommandesDuClient()
        {
            // Arrange : client avec une commande
            var clientId = await CreerClientViaApi();
            await CreerCommandeViaApi(clientId);

            // Act : récupérer les commandes du client
            var response = await _client.GetAsync($"/api/orders/customer/{clientId}");

            // Assert : statut 200
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var commandes = await response.Content.ReadFromJsonAsync<List<OrderResponse>>(_jsonOptions);
            Assert.NotNull(commandes);
            Assert.NotEmpty(commandes);
        }

        #endregion

        #region POST — Création de commande

        [Fact]
        public async Task Create_DoitRetournerCreated_QuandClientExiste()
        {
            // Arrange : client existant
            var clientId = await CreerClientViaApi();

            // Act : créer la commande
            var requete = new CreateOrderRequest { CustomerId = clientId };
            var response = await _client.PostAsJsonAsync("/api/orders", requete);

            // Assert : statut 201 Created
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var commande = await response.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
            Assert.Equal("Pending", commande!.Status);
        }

        [Fact]
        public async Task Create_DoitRetournerNotFound_QuandClientInexistant()
        {
            // Act : client inexistant
            var requete = new CreateOrderRequest { CustomerId = Guid.NewGuid() };
            var response = await _client.PostAsJsonAsync("/api/orders", requete);

            // Assert : statut 404 (ApplicationServiceException)
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST — Ajout d'articles

        [Fact]
        public async Task AddItem_DoitRetournerNoContent_QuandValide()
        {
            // Arrange : commande et produit existants
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);
            var produitId = await CreerProduitViaApi(30m);

            // Act : ajouter un article
            var response = await AjouterArticleViaApi(commande.OrderId, produitId, 2);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task AddItem_DoitRetournerNotFound_QuandProduitInexistant()
        {
            // Arrange : commande existante, produit inconnu
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);

            // Act : ajouter un article avec produit inexistant
            var response = await AjouterArticleViaApi(commande.OrderId, Guid.NewGuid());

            // Assert : statut 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddItem_DoitRetournerBadRequest_QuandProduitDejaDansCommande()
        {
            // Arrange : commande avec un article pour un produit
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);
            var produitId = await CreerProduitViaApi();
            await AjouterArticleViaApi(commande.OrderId, produitId);

            // Act : tenter d'ajouter le même produit
            var response = await AjouterArticleViaApi(commande.OrderId, produitId);

            // Assert : statut 400 (DomainException → produit déjà présent)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region DELETE — Retrait d'articles

        [Fact]
        public async Task RemoveItem_DoitRetournerNoContent_QuandArticleExiste()
        {
            // Arrange : commande avec un article
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);
            var produitId = await CreerProduitViaApi();
            await AjouterArticleViaApi(commande.OrderId, produitId);

            // Récupérer l'identifiant de l'article ajouté
            var getResponse = await _client.GetAsync($"/api/orders/{commande.OrderId}");
            var commandeDetail = await getResponse.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
            var articleId = commandeDetail!.Items.First().Id;

            // Act : retirer l'article
            var response = await _client.DeleteAsync($"/api/orders/{commande.OrderId}/items/{articleId}");

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion

        #region Transitions de statut

        [Fact]
        public async Task Confirm_DoitRetournerNoContent_QuandCommandeAvecArticles()
        {
            // Arrange : commande avec un article
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);
            var produitId = await CreerProduitViaApi();
            await AjouterArticleViaApi(commande.OrderId, produitId);

            // Act : confirmer
            var response = await _client.PutAsync($"/api/orders/{commande.OrderId}/confirm", null);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Confirm_DoitRetournerBadRequest_QuandSansArticles()
        {
            // Arrange : commande vide (pas d'articles)
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);

            // Act : tenter de confirmer
            var response = await _client.PutAsync($"/api/orders/{commande.OrderId}/confirm", null);

            // Assert : statut 400 (DomainException → pas d'articles)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Ship_DoitRetournerNoContent_QuandConfirmee()
        {
            // Arrange : commande confirmée
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);
            var produitId = await CreerProduitViaApi();
            await AjouterArticleViaApi(commande.OrderId, produitId);
            await _client.PutAsync($"/api/orders/{commande.OrderId}/confirm", null);

            // Act : expédier
            var response = await _client.PutAsync($"/api/orders/{commande.OrderId}/ship", null);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Deliver_DoitRetournerNoContent_QuandExpediee()
        {
            // Arrange : commande expédiée
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);
            var produitId = await CreerProduitViaApi();
            await AjouterArticleViaApi(commande.OrderId, produitId);
            await _client.PutAsync($"/api/orders/{commande.OrderId}/confirm", null);
            await _client.PutAsync($"/api/orders/{commande.OrderId}/ship", null);

            // Act : livrer
            var response = await _client.PutAsync($"/api/orders/{commande.OrderId}/deliver", null);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Cancel_DoitRetournerNoContent_QuandPossible()
        {
            // Arrange : commande en attente
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);

            // Act : annuler
            var response = await _client.PutAsync($"/api/orders/{commande.OrderId}/cancel", null);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Cancel_DoitRetournerBadRequest_QuandDejaLivree()
        {
            // Arrange : commande livrée
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);
            var produitId = await CreerProduitViaApi();
            await AjouterArticleViaApi(commande.OrderId, produitId);
            await _client.PutAsync($"/api/orders/{commande.OrderId}/confirm", null);
            await _client.PutAsync($"/api/orders/{commande.OrderId}/ship", null);
            await _client.PutAsync($"/api/orders/{commande.OrderId}/deliver", null);

            // Act : tenter d'annuler une commande déjà livrée
            var response = await _client.PutAsync($"/api/orders/{commande.OrderId}/cancel", null);

            // Assert : statut 400 (DomainException)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region DELETE — Suppression de commande

        [Fact]
        public async Task Delete_DoitRetournerNoContent()
        {
            // Arrange : commande existante
            var clientId = await CreerClientViaApi();
            var commande = await CreerCommandeViaApi(clientId);

            // Act : supprimer
            var response = await _client.DeleteAsync($"/api/orders/{commande.OrderId}");

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion
    }
}
