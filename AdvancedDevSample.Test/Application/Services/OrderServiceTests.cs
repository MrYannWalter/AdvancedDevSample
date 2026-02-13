using AdvancedDevSample.Application.DTOs.Order.OrderRequests;
using AdvancedDevSample.Application.Exceptions;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Repositories;

namespace AdvancedDevSample.Test.Application.Services
{
    /// <summary>
    /// Tests de composants du OrderService.
    /// Utilise les vrais repositories (InMemory*Repository + InMemoryDataStore)
    /// pour tester l'intégration service/repositories avec le même système de stockage.
    /// Le OrderService dépend de 3 repositories : Order, Customer, Product.
    /// </summary>
    public class OrderServiceTests
    {
        /// <summary>
        /// Crée un OrderService avec les 3 repositories et un store frais.
        /// Retourne aussi le store pour pouvoir y insérer des données de test.
        /// </summary>
        private static (OrderService service, InMemoryDataStore store) CreerService()
        {
            var store = new InMemoryDataStore();
            var orderRepo = new InMemoryOrderRepository(store);
            var customerRepo = new InMemoryCustomerRepository(store);
            var productRepo = new InMemoryProductRepository(store);
            var service = new OrderService(orderRepo, customerRepo, productRepo);
            return (service, store);
        }

        /// <summary>
        /// Insère un client valide dans le store et retourne son identifiant.
        /// </summary>
        private static Customer CreerClientDansStore(InMemoryDataStore store)
        {
            var client = new Customer("Jean", "Dupont", "jean@test.com");
            store.Customers[client.Id] = client;
            return client;
        }

        /// <summary>
        /// Insère un produit valide dans le store et retourne l'entité.
        /// </summary>
        private static Product CreerProduitDansStore(InMemoryDataStore store, decimal prix = 25m)
        {
            var produit = new Product("Produit Test", "Description test", prix);
            store.Products[produit.Id] = produit;
            return produit;
        }

        #region CreerCommande

        [Fact]
        public void CreerCommande_DoitCreerCommandeEnAttente_QuandClientExiste()
        {
            // Arrange : client dans le store
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);

            // Act : créer la commande
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Assert : commande en attente, liée au client
            Assert.NotEqual(Guid.Empty, commande.Id);
            Assert.Equal(client.Id, commande.CustomerId);
            Assert.Equal(OrderStatus.Pending, commande.Status);
        }

        [Fact]
        public void CreerCommande_DoitLeverApplicationServiceException_QuandClientInexistant()
        {
            // Arrange : store sans client
            var (service, _) = CreerService();

            // Act & Assert : client inexistant lève ApplicationServiceException
            var exception = Assert.Throws<ApplicationServiceException>(
                () => service.CreateOrder(new CreateOrderRequest { CustomerId = Guid.NewGuid() }));

            Assert.Equal(System.Net.HttpStatusCode.NotFound, exception.StatusCode);
        }

        #endregion

        #region ObtenirCommande

        [Fact]
        public void ObtenirCommande_DoitRetournerCommande_QuandIdExiste()
        {
            // Arrange : commande créée
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Act : récupérer
            var resultat = service.GetOrder(commande.Id);

            // Assert : la commande est retournée
            Assert.Equal(commande.Id, resultat.Id);
        }

        [Fact]
        public void ObtenirCommande_DoitLeverApplicationServiceException_QuandIdInexistant()
        {
            // Arrange : store vide
            var (service, _) = CreerService();

            // Act & Assert : identifiant inconnu
            Assert.Throws<ApplicationServiceException>(() => service.GetOrder(Guid.NewGuid()));
        }

        #endregion

        #region ObtenirToutesLesCommandes / Par client

        [Fact]
        public void ObtenirToutesLesCommandes_DoitRetournerListeComplete()
        {
            // Arrange : 2 commandes
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });
            service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Act : lister
            var commandes = service.GetAllOrders().ToList();

            // Assert : 2 commandes
            Assert.Equal(2, commandes.Count);
        }

        [Fact]
        public void ObtenirCommandesParClient_DoitRetournerCommandesDuClient()
        {
            // Arrange : 2 clients, 2 commandes pour le premier, 1 pour le second
            var (service, store) = CreerService();
            var clientA = CreerClientDansStore(store);
            var clientB = new Customer("Marie", "Martin", "marie@test.com");
            store.Customers[clientB.Id] = clientB;

            service.CreateOrder(new CreateOrderRequest { CustomerId = clientA.Id });
            service.CreateOrder(new CreateOrderRequest { CustomerId = clientA.Id });
            service.CreateOrder(new CreateOrderRequest { CustomerId = clientB.Id });

            // Act : récupérer les commandes du client A
            var commandesA = service.GetOrdersByCustomer(clientA.Id).ToList();

            // Assert : 2 commandes pour le client A
            Assert.Equal(2, commandesA.Count);
            Assert.All(commandesA, c => Assert.Equal(clientA.Id, c.CustomerId));
        }

        [Fact]
        public void ObtenirCommandesParClient_DoitRetournerListeVide_QuandAucuneCommande()
        {
            // Arrange : client sans commandes
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);

            // Act : récupérer les commandes
            var commandes = service.GetOrdersByCustomer(client.Id).ToList();

            // Assert : liste vide
            Assert.Empty(commandes);
        }

        #endregion

        #region AjouterArticle

        [Fact]
        public void AjouterArticle_DoitAjouterArticle_QuandCommandeEtProduitExistent()
        {
            // Arrange : commande et produit existants
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var produit = CreerProduitDansStore(store, 30m);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Act : ajouter un article
            service.AddItemToOrder(commande.Id, new AddOrderItemRequest
            {
                ProductId = produit.Id,
                Quantity = 2
            });

            // Assert : l'article est ajouté à la commande
            var commandeMaj = service.GetOrder(commande.Id);
            Assert.Single(commandeMaj.Items);
        }

        [Fact]
        public void AjouterArticle_DoitUtiliserPrixDuProduit()
        {
            // Arrange : produit à 42€
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var produit = CreerProduitDansStore(store, 42m);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Act : ajouter un article (le prix vient du produit, pas de la requête)
            service.AddItemToOrder(commande.Id, new AddOrderItemRequest
            {
                ProductId = produit.Id,
                Quantity = 1
            });

            // Assert : le prix unitaire de l'article est celui du produit (42€)
            var commandeMaj = service.GetOrder(commande.Id);
            var article = commandeMaj.Items.First();
            Assert.Equal(42m, article.UnitPrice);
        }

        [Fact]
        public void AjouterArticle_DoitLeverApplicationServiceException_QuandCommandeInexistante()
        {
            // Arrange : pas de commande
            var (service, store) = CreerService();
            var produit = CreerProduitDansStore(store);

            // Act & Assert : commande inexistante
            Assert.Throws<ApplicationServiceException>(
                () => service.AddItemToOrder(Guid.NewGuid(), new AddOrderItemRequest
                {
                    ProductId = produit.Id,
                    Quantity = 1
                }));
        }

        [Fact]
        public void AjouterArticle_DoitLeverApplicationServiceException_QuandProduitInexistant()
        {
            // Arrange : commande existante mais produit inconnu
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Act & Assert : produit inexistant
            Assert.Throws<ApplicationServiceException>(
                () => service.AddItemToOrder(commande.Id, new AddOrderItemRequest
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }));
        }

        #endregion

        #region RetirerArticle

        [Fact]
        public void RetirerArticle_DoitRetirerArticle_QuandArticleExiste()
        {
            // Arrange : commande avec un article
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var produit = CreerProduitDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });
            service.AddItemToOrder(commande.Id, new AddOrderItemRequest { ProductId = produit.Id, Quantity = 1 });
            var articleId = service.GetOrder(commande.Id).Items.First().Id;

            // Act : retirer l'article
            service.RemoveItemFromOrder(commande.Id, articleId);

            // Assert : la commande est vide
            Assert.Empty(service.GetOrder(commande.Id).Items);
        }

        [Fact]
        public void RetirerArticle_DoitLeverDomainException_QuandArticleIntrouvable()
        {
            // Arrange : commande sans l'article demandé
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Act & Assert : article inexistant
            Assert.Throws<DomainException>(
                () => service.RemoveItemFromOrder(commande.Id, Guid.NewGuid()));
        }

        #endregion

        #region Transitions de statut

        [Fact]
        public void ConfirmerCommande_DoitPasserStatutAConfirme()
        {
            // Arrange : commande avec un article
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var produit = CreerProduitDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });
            service.AddItemToOrder(commande.Id, new AddOrderItemRequest { ProductId = produit.Id, Quantity = 1 });

            // Act : confirmer
            service.ConfirmOrder(commande.Id);

            // Assert : statut Confirmed
            Assert.Equal(OrderStatus.Confirmed, service.GetOrder(commande.Id).Status);
        }

        [Fact]
        public void ConfirmerCommande_DoitLeverDomainException_QuandSansArticles()
        {
            // Arrange : commande vide
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Act & Assert : confirmation sans articles interdite
            Assert.Throws<DomainException>(() => service.ConfirmOrder(commande.Id));
        }

        [Fact]
        public void ExpedierCommande_DoitPasserStatutAExpedie()
        {
            // Arrange : commande confirmée
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var produit = CreerProduitDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });
            service.AddItemToOrder(commande.Id, new AddOrderItemRequest { ProductId = produit.Id, Quantity = 1 });
            service.ConfirmOrder(commande.Id);

            // Act : expédier
            service.ShipOrder(commande.Id);

            // Assert : statut Shipped
            Assert.Equal(OrderStatus.Shipped, service.GetOrder(commande.Id).Status);
        }

        [Fact]
        public void ExpedierCommande_DoitLeverDomainException_QuandPasConfirmee()
        {
            // Arrange : commande en attente (pas confirmée)
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Act & Assert : expédition impossible sans confirmation
            Assert.Throws<DomainException>(() => service.ShipOrder(commande.Id));
        }

        [Fact]
        public void LivrerCommande_DoitPasserStatutALivre()
        {
            // Arrange : commande expédiée
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var produit = CreerProduitDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });
            service.AddItemToOrder(commande.Id, new AddOrderItemRequest { ProductId = produit.Id, Quantity = 1 });
            service.ConfirmOrder(commande.Id);
            service.ShipOrder(commande.Id);

            // Act : livrer
            service.DeliverOrder(commande.Id);

            // Assert : statut Delivered
            Assert.Equal(OrderStatus.Delivered, service.GetOrder(commande.Id).Status);
        }

        [Fact]
        public void AnnulerCommande_DoitPasserStatutAAnnule()
        {
            // Arrange : commande en attente
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Act : annuler
            service.CancelOrder(commande.Id);

            // Assert : statut Cancelled
            Assert.Equal(OrderStatus.Cancelled, service.GetOrder(commande.Id).Status);
        }

        #endregion

        #region SupprimerCommande

        [Fact]
        public void SupprimerCommande_DoitRetirerCommandeDuDepot()
        {
            // Arrange : commande existante
            var (service, store) = CreerService();
            var client = CreerClientDansStore(store);
            var commande = service.CreateOrder(new CreateOrderRequest { CustomerId = client.Id });

            // Act : supprimer
            service.DeleteOrder(commande.Id);

            // Assert : la commande n'est plus trouvable
            Assert.Throws<ApplicationServiceException>(() => service.GetOrder(commande.Id));
        }

        #endregion
    }
}
