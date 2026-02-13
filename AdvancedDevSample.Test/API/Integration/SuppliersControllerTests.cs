using System.Net;
using System.Net.Http.Json;
using AdvancedDevSample.Application.DTOs.Supplier.SupplierRequests;
using AdvancedDevSample.Application.DTOs.Supplier.SupplierResponses;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AdvancedDevSample.Test.API.Integration
{
    /// <summary>
    /// Tests d'intégration du SuppliersController.
    /// Utilise WebApplicationFactory avec la vraie configuration DI.
    /// </summary>
    public class SuppliersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public SuppliersControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Crée un fournisseur via l'API et retourne sa réponse désérialisée.
        /// </summary>
        private async Task<SupplierResponse> CreerFournisseurViaApi(
            string nom = "Acme Corp", string email = "contact@acme.com", string phone = "0123456789")
        {
            var requete = new CreateSupplierRequest
            {
                CompanyName = nom,
                ContactEmail = email,
                Phone = phone
            };
            var response = await _client.PostAsJsonAsync("/api/suppliers", requete);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<SupplierResponse>())!;
        }

        #region GET

        [Fact]
        public async Task GetAll_DoitRetournerOk_AvecListeDeFournisseurs()
        {
            // Arrange : créer un fournisseur
            await CreerFournisseurViaApi();

            // Act : récupérer la liste
            var response = await _client.GetAsync("/api/suppliers");

            // Assert : statut 200
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var fournisseurs = await response.Content.ReadFromJsonAsync<List<SupplierResponse>>();
            Assert.NotNull(fournisseurs);
            Assert.NotEmpty(fournisseurs);
        }

        [Fact]
        public async Task GetById_DoitRetournerOk_QuandFournisseurExiste()
        {
            // Arrange : créer un fournisseur
            var fournisseur = await CreerFournisseurViaApi("Test SARL", "test@sarl.com");

            // Act : récupérer par identifiant
            var response = await _client.GetAsync($"/api/suppliers/{fournisseur.Id}");

            // Assert : statut 200, données correctes
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var resultat = await response.Content.ReadFromJsonAsync<SupplierResponse>();
            Assert.Equal("Test SARL", resultat!.CompanyName);
        }

        [Fact]
        public async Task GetById_DoitRetournerNotFound_QuandFournisseurInexistant()
        {
            // Act : identifiant inconnu
            var response = await _client.GetAsync($"/api/suppliers/{Guid.NewGuid()}");

            // Assert : statut 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST

        [Fact]
        public async Task Create_DoitRetournerCreated_QuandRequeteValide()
        {
            // Arrange : requête valide
            var requete = new CreateSupplierRequest
            {
                CompanyName = "Nouvelle Société",
                ContactEmail = "nouvelle@societe.com",
                Phone = "0987654321"
            };

            // Act : créer le fournisseur
            var response = await _client.PostAsJsonAsync("/api/suppliers", requete);

            // Assert : statut 201 Created
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var fournisseur = await response.Content.ReadFromJsonAsync<SupplierResponse>();
            Assert.NotNull(fournisseur);
            Assert.Equal("Nouvelle Société", fournisseur.CompanyName);
            Assert.True(fournisseur.IsActive);
        }

        [Fact]
        public async Task Create_DoitRetournerBadRequest_QuandDonneesInvalides()
        {
            // Arrange : requête avec nom de société null (DomainException → 400)
            var requete = new { CompanyName = (string?)null, ContactEmail = "e@e.com", Phone = "111" };

            // Act : tenter la création
            var response = await _client.PostAsJsonAsync("/api/suppliers", requete);

            // Assert : statut 400
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT

        [Fact]
        public async Task Update_DoitRetournerNoContent_QuandFournisseurExiste()
        {
            // Arrange : fournisseur existant
            var fournisseur = await CreerFournisseurViaApi();
            var requete = new UpdateSupplierRequest
            {
                CompanyName = "Nouveau Nom",
                ContactEmail = "nouveau@email.com",
                Phone = "999"
            };

            // Act : mettre à jour
            var response = await _client.PutAsJsonAsync($"/api/suppliers/{fournisseur.Id}", requete);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Update_DoitRetournerNotFound_QuandFournisseurInexistant()
        {
            // Act : identifiant inconnu
            var requete = new UpdateSupplierRequest
            {
                CompanyName = "A",
                ContactEmail = "a@a.com",
                Phone = ""
            };
            var response = await _client.PutAsJsonAsync($"/api/suppliers/{Guid.NewGuid()}", requete);

            // Assert : statut 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region DELETE

        [Fact]
        public async Task Delete_DoitRetournerNoContent_QuandFournisseurExiste()
        {
            // Arrange : fournisseur existant
            var fournisseur = await CreerFournisseurViaApi("Suppr SARL", "suppr@sarl.com");

            // Act : supprimer
            var response = await _client.DeleteAsync($"/api/suppliers/{fournisseur.Id}");

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_DoitRetournerNotFound_QuandFournisseurInexistant()
        {
            // Act : identifiant inconnu
            var response = await _client.DeleteAsync($"/api/suppliers/{Guid.NewGuid()}");

            // Assert : statut 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Activation / Désactivation

        [Fact]
        public async Task Activate_DoitRetournerNoContent()
        {
            // Arrange : fournisseur désactivé
            var fournisseur = await CreerFournisseurViaApi("Act SARL", "act@sarl.com");
            await _client.PutAsync($"/api/suppliers/{fournisseur.Id}/deactivate", null);

            // Act : réactiver
            var response = await _client.PutAsync($"/api/suppliers/{fournisseur.Id}/activate", null);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Deactivate_DoitRetournerNoContent()
        {
            // Arrange : fournisseur actif
            var fournisseur = await CreerFournisseurViaApi("Deact SARL", "deact@sarl.com");

            // Act : désactiver
            var response = await _client.PutAsync($"/api/suppliers/{fournisseur.Id}/deactivate", null);

            // Assert : statut 204
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion
    }
}
