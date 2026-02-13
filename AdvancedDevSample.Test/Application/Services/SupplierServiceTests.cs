using AdvancedDevSample.Application.DTOs.Supplier.SupplierRequests;
using AdvancedDevSample.Application.Exceptions;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Repositories;

namespace AdvancedDevSample.Test.Application.Services
{
    /// <summary>
    /// Tests de composants du SupplierService.
    /// Utilise les vrais repositories (InMemorySupplierRepository + InMemoryDataStore)
    /// pour tester l'intégration service/repository avec le même système de stockage.
    /// </summary>
    public class SupplierServiceTests
    {
        /// <summary>
        /// Crée un service avec un store frais pour chaque test (isolation).
        /// </summary>
        private static SupplierService CreerService()
        {
            var store = new InMemoryDataStore();
            var repo = new InMemorySupplierRepository(store);
            return new SupplierService(repo);
        }

        #region CreerFournisseur

        [Fact]
        public void CreerFournisseur_DoitAjouterFournisseurAuDepot_QuandRequeteValide()
        {
            // Arrange : service et requête valide
            var service = CreerService();
            var requete = new CreateSupplierRequest
            {
                CompanyName = "Acme Corp",
                ContactEmail = "contact@acme.com",
                Phone = "0123456789"
            };

            // Act : créer le fournisseur
            var fournisseur = service.CreateSupplier(requete);

            // Assert : le fournisseur est créé et récupérable
            Assert.NotEqual(Guid.Empty, fournisseur.Id);
            Assert.Equal("Acme Corp", fournisseur.CompanyName);
            var recupere = service.GetSupplier(fournisseur.Id);
            Assert.Equal(fournisseur.Id, recupere.Id);
        }

        [Fact]
        public void CreerFournisseur_DoitLeverDomainException_QuandNomSocieteEstNull()
        {
            // Arrange : requête avec nom de société null
            var service = CreerService();
            var requete = new CreateSupplierRequest
            {
                CompanyName = null!,
                ContactEmail = "email@test.com",
                Phone = "0123"
            };

            // Act & Assert : la DomainException est propagée
            Assert.Throws<DomainException>(() => service.CreateSupplier(requete));
        }

        #endregion

        #region ObtenirFournisseur

        [Fact]
        public void ObtenirFournisseur_DoitRetournerFournisseur_QuandIdExiste()
        {
            // Arrange : créer un fournisseur
            var service = CreerService();
            var fournisseur = service.CreateSupplier(new CreateSupplierRequest
            {
                CompanyName = "Acme",
                ContactEmail = "acme@test.com",
                Phone = "111"
            });

            // Act : récupérer
            var resultat = service.GetSupplier(fournisseur.Id);

            // Assert : le bon fournisseur
            Assert.Equal(fournisseur.Id, resultat.Id);
            Assert.Equal("Acme", resultat.CompanyName);
        }

        [Fact]
        public void ObtenirFournisseur_DoitLeverApplicationServiceException_QuandIdInexistant()
        {
            // Arrange : service vide
            var service = CreerService();

            // Act & Assert : identifiant inconnu
            var exception = Assert.Throws<ApplicationServiceException>(
                () => service.GetSupplier(Guid.NewGuid()));

            Assert.Equal(System.Net.HttpStatusCode.NotFound, exception.StatusCode);
        }

        #endregion

        #region ObtenirTousLesFournisseurs

        [Fact]
        public void ObtenirTousLesFournisseurs_DoitRetournerListeComplete()
        {
            // Arrange : créer 2 fournisseurs
            var service = CreerService();
            service.CreateSupplier(new CreateSupplierRequest { CompanyName = "A", ContactEmail = "a@a.com", Phone = "" });
            service.CreateSupplier(new CreateSupplierRequest { CompanyName = "B", ContactEmail = "b@b.com", Phone = "" });

            // Act : lister
            var fournisseurs = service.GetAllSuppliers().ToList();

            // Assert : 2 fournisseurs
            Assert.Equal(2, fournisseurs.Count);
        }

        [Fact]
        public void ObtenirTousLesFournisseurs_DoitRetournerListeVide_QuandAucunFournisseur()
        {
            // Arrange : store vide
            var service = CreerService();

            // Act & Assert : liste vide
            Assert.Empty(service.GetAllSuppliers());
        }

        #endregion

        #region MettreAJourFournisseur

        [Fact]
        public void MettreAJourFournisseur_DoitModifierLesDonnees_QuandValide()
        {
            // Arrange : fournisseur existant
            var service = CreerService();
            var fournisseur = service.CreateSupplier(new CreateSupplierRequest
            {
                CompanyName = "Ancien",
                ContactEmail = "ancien@test.com",
                Phone = "000"
            });

            // Act : mettre à jour
            service.UpdateSupplier(fournisseur.Id, new UpdateSupplierRequest
            {
                CompanyName = "Nouveau",
                ContactEmail = "nouveau@test.com",
                Phone = "999"
            });

            // Assert : données mises à jour
            var maj = service.GetSupplier(fournisseur.Id);
            Assert.Equal("Nouveau", maj.CompanyName);
            Assert.Equal("nouveau@test.com", maj.ContactEmail);
            Assert.Equal("999", maj.Phone);
        }

        [Fact]
        public void MettreAJourFournisseur_DoitLeverApplicationServiceException_QuandInexistant()
        {
            // Arrange : service vide
            var service = CreerService();

            // Act & Assert : identifiant inconnu
            Assert.Throws<ApplicationServiceException>(
                () => service.UpdateSupplier(Guid.NewGuid(), new UpdateSupplierRequest
                {
                    CompanyName = "A",
                    ContactEmail = "a@a.com",
                    Phone = ""
                }));
        }

        #endregion

        #region SupprimerFournisseur

        [Fact]
        public void SupprimerFournisseur_DoitRetirerFournisseurDuDepot()
        {
            // Arrange : fournisseur existant
            var service = CreerService();
            var fournisseur = service.CreateSupplier(new CreateSupplierRequest
            {
                CompanyName = "Acme",
                ContactEmail = "acme@test.com",
                Phone = "111"
            });

            // Act : supprimer
            service.DeleteSupplier(fournisseur.Id);

            // Assert : le fournisseur n'est plus trouvable
            Assert.Throws<ApplicationServiceException>(() => service.GetSupplier(fournisseur.Id));
        }

        [Fact]
        public void SupprimerFournisseur_DoitLeverApplicationServiceException_QuandInexistant()
        {
            // Arrange : service vide
            var service = CreerService();

            // Act & Assert : suppression d'un fournisseur inexistant
            Assert.Throws<ApplicationServiceException>(
                () => service.DeleteSupplier(Guid.NewGuid()));
        }

        #endregion

        #region Activation / Désactivation

        [Fact]
        public void ActiverFournisseur_DoitRendreFournisseurActif()
        {
            // Arrange : fournisseur désactivé
            var service = CreerService();
            var fournisseur = service.CreateSupplier(new CreateSupplierRequest
            {
                CompanyName = "Acme",
                ContactEmail = "acme@test.com",
                Phone = "111"
            });
            service.DeactivateSupplier(fournisseur.Id);

            // Act : réactiver
            service.ActivateSupplier(fournisseur.Id);

            // Assert : actif
            Assert.True(service.GetSupplier(fournisseur.Id).IsActive);
        }

        [Fact]
        public void DesactiverFournisseur_DoitRendreFournisseurInactif()
        {
            // Arrange : fournisseur actif
            var service = CreerService();
            var fournisseur = service.CreateSupplier(new CreateSupplierRequest
            {
                CompanyName = "Acme",
                ContactEmail = "acme@test.com",
                Phone = "111"
            });

            // Act : désactiver
            service.DeactivateSupplier(fournisseur.Id);

            // Assert : inactif
            Assert.False(service.GetSupplier(fournisseur.Id).IsActive);
        }

        #endregion
    }
}
