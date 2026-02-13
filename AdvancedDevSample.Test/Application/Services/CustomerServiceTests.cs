using AdvancedDevSample.Application.DTOs.Customer.CustomerRequests;
using AdvancedDevSample.Application.Exceptions;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Repositories;

namespace AdvancedDevSample.Test.Application.Services
{
    /// <summary>
    /// Tests de composants du CustomerService.
    /// Utilise les vrais repositories (InMemoryCustomerRepository + InMemoryDataStore)
    /// pour tester l'intégration service/repository avec le même système de stockage.
    /// </summary>
    public class CustomerServiceTests
    {
        /// <summary>
        /// Crée un service avec un store frais pour chaque test (isolation).
        /// </summary>
        private static CustomerService CreerService()
        {
            var store = new InMemoryDataStore();
            var repo = new InMemoryCustomerRepository(store);
            return new CustomerService(repo);
        }

        #region CreerClient

        [Fact]
        public void CreerClient_DoitAjouterClientAuDepot_QuandRequeteValide()
        {
            // Arrange : service et requête valide
            var service = CreerService();
            var requete = new CreateCustomerRequest
            {
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean@test.com"
            };

            // Act : créer le client
            var client = service.CreateCustomer(requete);

            // Assert : le client est créé et récupérable
            Assert.NotEqual(Guid.Empty, client.Id);
            Assert.Equal("Jean", client.FirstName);
            var clientRecupere = service.GetCustomer(client.Id);
            Assert.Equal(client.Id, clientRecupere.Id);
        }

        [Fact]
        public void CreerClient_DoitLeverDomainException_QuandPrenomEstNull()
        {
            // Arrange : requête avec prénom null
            var service = CreerService();
            var requete = new CreateCustomerRequest
            {
                FirstName = null!,
                LastName = "Dupont",
                Email = "email@test.com"
            };

            // Act & Assert : la DomainException du constructeur Customer est propagée
            Assert.Throws<DomainException>(() => service.CreateCustomer(requete));
        }

        #endregion

        #region ObtenirClient

        [Fact]
        public void ObtenirClient_DoitRetournerClient_QuandIdExiste()
        {
            // Arrange : créer un client
            var service = CreerService();
            var client = service.CreateCustomer(new CreateCustomerRequest
            {
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean@test.com"
            });

            // Act : récupérer le client
            var resultat = service.GetCustomer(client.Id);

            // Assert : le client retourné est le bon
            Assert.Equal(client.Id, resultat.Id);
            Assert.Equal("Jean", resultat.FirstName);
        }

        [Fact]
        public void ObtenirClient_DoitLeverApplicationServiceException_QuandIdInexistant()
        {
            // Arrange : service vide
            var service = CreerService();

            // Act & Assert : identifiant inconnu lève ApplicationServiceException
            var exception = Assert.Throws<ApplicationServiceException>(
                () => service.GetCustomer(Guid.NewGuid()));

            Assert.Equal(System.Net.HttpStatusCode.NotFound, exception.StatusCode);
        }

        #endregion

        #region ObtenirTousLesClients

        [Fact]
        public void ObtenirTousLesClients_DoitRetournerListeComplete()
        {
            // Arrange : créer 2 clients
            var service = CreerService();
            service.CreateCustomer(new CreateCustomerRequest { FirstName = "A", LastName = "B", Email = "a@b.com" });
            service.CreateCustomer(new CreateCustomerRequest { FirstName = "C", LastName = "D", Email = "c@d.com" });

            // Act : lister
            var clients = service.GetAllCustomers().ToList();

            // Assert : 2 clients
            Assert.Equal(2, clients.Count);
        }

        [Fact]
        public void ObtenirTousLesClients_DoitRetournerListeVide_QuandAucunClient()
        {
            // Arrange : store vide
            var service = CreerService();

            // Act & Assert : liste vide
            Assert.Empty(service.GetAllCustomers());
        }

        #endregion

        #region MettreAJourClient

        [Fact]
        public void MettreAJourClient_DoitModifierLesDonnees_QuandValide()
        {
            // Arrange : client existant
            var service = CreerService();
            var client = service.CreateCustomer(new CreateCustomerRequest
            {
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean@test.com"
            });

            // Act : mettre à jour
            service.UpdateCustomer(client.Id, new UpdateCustomerRequest
            {
                FirstName = "Pierre",
                LastName = "Martin",
                Email = "pierre@test.com"
            });

            // Assert : les données sont mises à jour
            var clientMaj = service.GetCustomer(client.Id);
            Assert.Equal("Pierre", clientMaj.FirstName);
            Assert.Equal("Martin", clientMaj.LastName);
            Assert.Equal("pierre@test.com", clientMaj.Email);
        }

        [Fact]
        public void MettreAJourClient_DoitLeverApplicationServiceException_QuandClientInexistant()
        {
            // Arrange : service vide
            var service = CreerService();

            // Act & Assert : identifiant inconnu
            Assert.Throws<ApplicationServiceException>(
                () => service.UpdateCustomer(Guid.NewGuid(), new UpdateCustomerRequest
                {
                    FirstName = "A",
                    LastName = "B",
                    Email = "a@b.com"
                }));
        }

        #endregion

        #region SupprimerClient

        [Fact]
        public void SupprimerClient_DoitRetirerClientDuDepot()
        {
            // Arrange : client existant
            var service = CreerService();
            var client = service.CreateCustomer(new CreateCustomerRequest
            {
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean@test.com"
            });

            // Act : supprimer
            service.DeleteCustomer(client.Id);

            // Assert : le client n'est plus trouvable
            Assert.Throws<ApplicationServiceException>(() => service.GetCustomer(client.Id));
        }

        [Fact]
        public void SupprimerClient_DoitLeverApplicationServiceException_QuandClientInexistant()
        {
            // Arrange : service vide
            var service = CreerService();

            // Act & Assert : suppression d'un client inexistant lève une exception
            // (car DeleteCustomer appelle GetCustomer en interne)
            Assert.Throws<ApplicationServiceException>(
                () => service.DeleteCustomer(Guid.NewGuid()));
        }

        #endregion

        #region Activation / Désactivation

        [Fact]
        public void ActiverClient_DoitRendreClientActif()
        {
            // Arrange : client désactivé
            var service = CreerService();
            var client = service.CreateCustomer(new CreateCustomerRequest
            {
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean@test.com"
            });
            service.DeactivateCustomer(client.Id);

            // Act : réactiver
            service.ActivateCustomer(client.Id);

            // Assert : le client est actif
            Assert.True(service.GetCustomer(client.Id).IsActive);
        }

        [Fact]
        public void DesactiverClient_DoitRendreClientInactif()
        {
            // Arrange : client actif
            var service = CreerService();
            var client = service.CreateCustomer(new CreateCustomerRequest
            {
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean@test.com"
            });

            // Act : désactiver
            service.DeactivateCustomer(client.Id);

            // Assert : le client est inactif
            Assert.False(service.GetCustomer(client.Id).IsActive);
        }

        #endregion
    }
}
