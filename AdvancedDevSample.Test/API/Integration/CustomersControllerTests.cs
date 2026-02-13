using System.Net;
using System.Net.Http.Json;
using AdvancedDevSample.Application.DTOs.Customer.CustomerRequests;
using AdvancedDevSample.Application.DTOs.Customer.CustomerResponses;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AdvancedDevSample.Test.API.Integration
{
    /// <summary>
    /// Tests d'intégration du CustomersController.
    /// Utilise WebApplicationFactory avec la vraie configuration DI.
    /// </summary>
    public class CustomersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CustomersControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Crée un client via l'API et retourne sa réponse désérialisée.
        /// </summary>
        private async Task<CustomerResponse> CreerClientViaApi(
            string prenom = "Jean", string nom = "Dupont", string email = "jean@test.com")
        {
            var requete = new CreateCustomerRequest
            {
                FirstName = prenom,
                LastName = nom,
                Email = email
            };
            var response = await _client.PostAsJsonAsync("/api/customers", requete);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CustomerResponse>())!;
        }

        #region GET

        [Fact]
        public async Task GetAll_DoitRetournerOk_AvecListeDeClients()
        {
            // Arrange : créer un client
            await CreerClientViaApi();

            // Act : récupérer la liste
            var response = await _client.GetAsync("/api/customers");

            // Assert : statut 200
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var clients = await response.Content.ReadFromJsonAsync<List<CustomerResponse>>();
            Assert.NotNull(clients);
            Assert.NotEmpty(clients);
        }

        [Fact]
        public async Task GetById_DoitRetournerOk_QuandClientExiste()
        {
            // Arrange : créer un client
            var client = await CreerClientViaApi("Marie", "Martin", "marie@test.com");

            // Act : récupérer par identifiant
            var response = await _client.GetAsync($"/api/customers/{client.Id}");

            // Assert : statut 200, données correctes
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var resultat = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            Assert.Equal("Marie", resultat!.FirstName);
        }

        [Fact]
        public async Task GetById_DoitRetournerNotFound_QuandClientInexistant()
        {
            // Act : identifiant inconnu
            var response = await _client.GetAsync($"/api/customers/{Guid.NewGuid()}");

            // Assert : statut 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST

        [Fact]
        public async Task Create_DoitRetournerCreated_QuandRequeteValide()
        {
            // Arrange : requête valide
            var requete = new CreateCustomerRequest
            {
                FirstName = "Pierre",
                LastName = "Durand",
                Email = "pierre@test.com"
            };

            // Act : créer le client
            var response = await _client.PostAsJsonAsync("/api/customers", requete);

            // Assert : statut 201 Created
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var client = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            Assert.NotNull(client);
            Assert.Equal("Pierre", client.FirstName);
            Assert.True(client.IsActive);
        }

        [Fact]
        public async Task Create_DoitRetournerBadRequest_QuandDonneesInvalides()
        {
            // Arrange : requête avec nom null (DomainException → 400)
            var requete = new { FirstName = (string?)null, LastName = "Nom", Email = "e@e.com" };

            // Act : tenter la création
            var response = await _client.PostAsJsonAsync("/api/customers", requete);

            // Assert : statut 400
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT

        [Fact]
        public async Task Update_DoitRetournerNoContent_QuandClientExiste()
        {
            // Arrange : client existant
            var client = await CreerClientViaApi();
            var requete = new UpdateCustomerRequest
            {
                FirstName = "Nouveau",
                LastName = "Nom",
                Email = "nouveau@test.com"
            };

            // Act : mettre à jour
            var response = await _client.PutAsJsonAsync($"/api/customers/{client.Id}", requete);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Update_DoitRetournerNotFound_QuandClientInexistant()
        {
            // Act : identifiant inconnu
            var requete = new UpdateCustomerRequest
            {
                FirstName = "A",
                LastName = "B",
                Email = "a@b.com"
            };
            var response = await _client.PutAsJsonAsync($"/api/customers/{Guid.NewGuid()}", requete);

            // Assert : statut 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region DELETE

        [Fact]
        public async Task Delete_DoitRetournerNoContent_QuandClientExiste()
        {
            // Arrange : client existant
            var client = await CreerClientViaApi("Suppr", "Client", "suppr@test.com");

            // Act : supprimer
            var response = await _client.DeleteAsync($"/api/customers/{client.Id}");

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_DoitRetournerNotFound_QuandClientInexistant()
        {
            // Act : identifiant inconnu
            var response = await _client.DeleteAsync($"/api/customers/{Guid.NewGuid()}");

            // Assert : statut 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Activation / Désactivation

        [Fact]
        public async Task Activate_DoitRetournerNoContent()
        {
            // Arrange : client désactivé
            var client = await CreerClientViaApi("Act", "Client", "act@test.com");
            await _client.PutAsync($"/api/customers/{client.Id}/deactivate", null);

            // Act : réactiver
            var response = await _client.PutAsync($"/api/customers/{client.Id}/activate", null);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Deactivate_DoitRetournerNoContent()
        {
            // Arrange : client actif
            var client = await CreerClientViaApi("Deact", "Client", "deact@test.com");

            // Act : désactiver
            var response = await _client.PutAsync($"/api/customers/{client.Id}/deactivate", null);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion
    }
}
