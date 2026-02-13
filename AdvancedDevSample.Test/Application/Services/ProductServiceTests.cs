using AdvancedDevSample.Application.DTOs.Product.ProductRequests;
using AdvancedDevSample.Application.Exceptions;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Repositories;

namespace AdvancedDevSample.Test.Application.Services
{
    /// <summary>
    /// Tests de composants du ProductService.
    /// Utilise les vrais repositories (InMemoryProductRepository + InMemoryDataStore)
    /// pour tester l'intégration service/repository avec le même système de stockage.
    /// </summary>
    public class ProductServiceTests
    {
        /// <summary>
        /// Crée un service avec un store frais pour chaque test (isolation).
        /// </summary>
        private static ProductService CreerService()
        {
            var store = new InMemoryDataStore();
            var repo = new InMemoryProductRepository(store);
            return new ProductService(repo);
        }

        #region CreerProduit

        [Fact]
        public void CreerProduit_DoitAjouterProduitAuDepot_QuandRequeteValide()
        {
            // Arrange : service et requête valide
            var service = CreerService();
            var requete = new CreateProductRequest
            {
                Name = "Clavier",
                Description = "Clavier mécanique",
                Price = 89.99m
            };

            // Act : créer le produit
            var produit = service.CreateProduct(requete);

            // Assert : le produit est créé et récupérable
            Assert.NotEqual(Guid.Empty, produit.Id);
            Assert.Equal("Clavier", produit.Name);
            var produitRecupere = service.GetProduct(produit.Id);
            Assert.Equal(produit.Id, produitRecupere.Id);
        }

        [Fact]
        public void CreerProduit_DoitLeverDomainException_QuandNomEstNull()
        {
            // Arrange : requête avec nom null
            var service = CreerService();
            var requete = new CreateProductRequest
            {
                Name = null!,
                Description = "Description",
                Price = 10m
            };

            // Act & Assert : la DomainException du constructeur Product est propagée
            Assert.Throws<DomainException>(() => service.CreateProduct(requete));
        }

        #endregion

        #region ObtenirProduit

        [Fact]
        public void ObtenirProduit_DoitRetournerProduit_QuandIdExiste()
        {
            // Arrange : créer un produit
            var service = CreerService();
            var produit = service.CreateProduct(new CreateProductRequest
            {
                Name = "Souris",
                Description = "Souris sans fil",
                Price = 29.99m
            });

            // Act : récupérer le produit
            var resultat = service.GetProduct(produit.Id);

            // Assert : le produit retourné est le bon
            Assert.Equal(produit.Id, resultat.Id);
            Assert.Equal("Souris", resultat.Name);
        }

        [Fact]
        public void ObtenirProduit_DoitLeverApplicationServiceException_QuandIdInexistant()
        {
            // Arrange : service vide
            var service = CreerService();

            // Act & Assert : un identifiant inconnu lève ApplicationServiceException
            var exception = Assert.Throws<ApplicationServiceException>(
                () => service.GetProduct(Guid.NewGuid()));

            Assert.Equal(System.Net.HttpStatusCode.NotFound, exception.StatusCode);
        }

        #endregion

        #region ObtenirTousLesProduits

        [Fact]
        public void ObtenirTousLesProduits_DoitRetournerListeComplete()
        {
            // Arrange : créer 3 produits
            var service = CreerService();
            service.CreateProduct(new CreateProductRequest { Name = "A", Description = "a", Price = 1m });
            service.CreateProduct(new CreateProductRequest { Name = "B", Description = "b", Price = 2m });
            service.CreateProduct(new CreateProductRequest { Name = "C", Description = "c", Price = 3m });

            // Act : lister tous les produits
            var produits = service.GetAllProducts().ToList();

            // Assert : 3 produits retournés
            Assert.Equal(3, produits.Count);
        }

        [Fact]
        public void ObtenirTousLesProduits_DoitRetournerListeVide_QuandAucunProduit()
        {
            // Arrange : store vide
            var service = CreerService();

            // Act : lister les produits
            var produits = service.GetAllProducts().ToList();

            // Assert : liste vide
            Assert.Empty(produits);
        }

        #endregion

        #region ChangerPrixProduit

        [Fact]
        public void ChangerPrixProduit_DoitMettreAJourLePrix_QuandPrixValide()
        {
            // Arrange : produit à 50€
            var service = CreerService();
            var produit = service.CreateProduct(new CreateProductRequest
            {
                Name = "Produit",
                Description = "Desc",
                Price = 50m
            });

            // Act : changer le prix à 75€
            service.ChangeProductPrice(produit.Id, new ChangePriceRequest { NewPrice = 75m });

            // Assert : le prix est mis à jour dans le store
            var produitMaj = service.GetProduct(produit.Id);
            Assert.Equal(75m, produitMaj.Price);
        }

        [Fact]
        public void ChangerPrixProduit_DoitLeverApplicationServiceException_QuandProduitInexistant()
        {
            // Arrange : service vide
            var service = CreerService();

            // Act & Assert : produit inconnu lève ApplicationServiceException
            Assert.Throws<ApplicationServiceException>(
                () => service.ChangeProductPrice(Guid.NewGuid(), new ChangePriceRequest { NewPrice = 10m }));
        }

        #endregion

        #region MettreAJourProduit

        [Fact]
        public void MettreAJourProduit_DoitModifierNomEtDescription_QuandValide()
        {
            // Arrange : produit existant
            var service = CreerService();
            var produit = service.CreateProduct(new CreateProductRequest
            {
                Name = "Ancien",
                Description = "Ancienne desc",
                Price = 10m
            });

            // Act : mettre à jour
            service.UpdateProduct(produit.Id, new UpdateProductRequest
            {
                Name = "Nouveau",
                Description = "Nouvelle desc"
            });

            // Assert : les données sont mises à jour
            var produitMaj = service.GetProduct(produit.Id);
            Assert.Equal("Nouveau", produitMaj.Name);
            Assert.Equal("Nouvelle desc", produitMaj.Description);
        }

        #endregion

        #region SupprimerProduit

        [Fact]
        public void SupprimerProduit_DoitRetirerProduitDuDepot()
        {
            // Arrange : produit existant
            var service = CreerService();
            var produit = service.CreateProduct(new CreateProductRequest
            {
                Name = "Produit",
                Description = "Desc",
                Price = 10m
            });

            // Act : supprimer le produit
            service.DeleteProduct(produit.Id);

            // Assert : le produit n'est plus trouvable
            Assert.Throws<ApplicationServiceException>(() => service.GetProduct(produit.Id));
        }

        #endregion

        #region Activation / Désactivation

        [Fact]
        public void ActiverProduit_DoitRendreProduitActif()
        {
            // Arrange : produit désactivé
            var service = CreerService();
            var produit = service.CreateProduct(new CreateProductRequest
            {
                Name = "Produit",
                Description = "Desc",
                Price = 10m
            });
            service.DeactivateProduct(produit.Id);

            // Act : réactiver
            service.ActivateProduct(produit.Id);

            // Assert : le produit est actif
            var produitMaj = service.GetProduct(produit.Id);
            Assert.True(produitMaj.IsActive);
        }

        [Fact]
        public void DesactiverProduit_DoitRendreProduitInactif()
        {
            // Arrange : produit actif
            var service = CreerService();
            var produit = service.CreateProduct(new CreateProductRequest
            {
                Name = "Produit",
                Description = "Desc",
                Price = 10m
            });

            // Act : désactiver
            service.DeactivateProduct(produit.Id);

            // Assert : le produit est inactif
            var produitMaj = service.GetProduct(produit.Id);
            Assert.False(produitMaj.IsActive);
        }

        #endregion
    }
}
