using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Test.Domain.Entities
{
    /// <summary>
    /// Tests unitaires de l'entité Order (agrégat commande).
    /// Vérifie les règles métier : construction, gestion des articles,
    /// calcul du total, machine à états (Pending → Confirmed → Shipped → Delivered / Cancelled).
    /// </summary>
    public class OrderTests
    {
        // Identifiant de client valide réutilisé dans les tests
        private readonly Guid _clientId = Guid.NewGuid();

        #region Helpers

        /// <summary>
        /// Crée une commande en attente avec un article pour faciliter les tests de transition.
        /// </summary>
        private Order CreerCommandeAvecArticle()
        {
            var commande = new Order(_clientId);
            commande.AddItem(Guid.NewGuid(), 1, 10m);
            return commande;
        }

        /// <summary>
        /// Crée une commande confirmée (avec article) pour tester Ship/Deliver/Cancel.
        /// </summary>
        private Order CreerCommandeConfirmee()
        {
            var commande = CreerCommandeAvecArticle();
            commande.Confirm();
            return commande;
        }

        #endregion

        #region Constructeur

        [Fact]
        public void Constructeur_DoitCreerCommande_QuandClientIdValide()
        {
            // Act : création de la commande
            var commande = new Order(_clientId);

            // Assert : propriétés initialisées correctement
            Assert.NotEqual(Guid.Empty, commande.Id);
            Assert.Equal(_clientId, commande.CustomerId);
            Assert.Equal(OrderStatus.Pending, commande.Status);
            Assert.Empty(commande.Items);
            // La date de commande doit être proche de maintenant (tolérance 5 secondes)
            Assert.True((DateTime.UtcNow - commande.OrderDate).TotalSeconds < 5);
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandClientIdEstVide()
        {
            // Act & Assert : Guid.Empty déclenche une DomainException
            Assert.Throws<DomainException>(() => new Order(Guid.Empty));
        }

        #endregion

        #region AjouterArticle

        [Fact]
        public void AjouterArticle_DoitAjouterArticle_QuandCommandeEnAttente()
        {
            // Arrange : commande en attente
            var commande = new Order(_clientId);
            var produitId = Guid.NewGuid();

            // Act : ajouter un article
            commande.AddItem(produitId, 2, 15m);

            // Assert : un article est ajouté
            Assert.Single(commande.Items);
            var article = commande.Items.First();
            Assert.Equal(produitId, article.ProductId);
            Assert.Equal(2, article.Quantity);
            Assert.Equal(15m, article.UnitPrice);
        }

        [Fact]
        public void AjouterArticle_DoitLeverDomainException_QuandCommandePasEnAttente()
        {
            // Arrange : commande confirmée
            var commande = CreerCommandeConfirmee();

            // Act & Assert : ajout interdit après confirmation
            Assert.Throws<DomainException>(
                () => commande.AddItem(Guid.NewGuid(), 1, 10m));
        }

        [Fact]
        public void AjouterArticle_DoitLeverDomainException_QuandProduitDejaPresent()
        {
            // Arrange : commande avec un article pour un produit donné
            var commande = new Order(_clientId);
            var produitId = Guid.NewGuid();
            commande.AddItem(produitId, 1, 10m);

            // Act & Assert : ajouter le même produit une deuxième fois est interdit
            Assert.Throws<DomainException>(
                () => commande.AddItem(produitId, 2, 10m));
        }

        [Fact]
        public void AjouterArticle_DoitLeverDomainException_QuandCommandeAnnulee()
        {
            // Arrange : commande annulée
            var commande = new Order(_clientId);
            commande.Cancel();

            // Act & Assert : ajout interdit sur une commande annulée
            Assert.Throws<DomainException>(
                () => commande.AddItem(Guid.NewGuid(), 1, 10m));
        }

        #endregion

        #region RetirerArticle

        [Fact]
        public void RetirerArticle_DoitRetirerArticle_QuandCommandeEnAttente()
        {
            // Arrange : commande avec un article
            var commande = CreerCommandeAvecArticle();
            var articleId = commande.Items.First().Id;

            // Act : retirer l'article
            commande.RemoveItem(articleId);

            // Assert : la commande est vide
            Assert.Empty(commande.Items);
        }

        [Fact]
        public void RetirerArticle_DoitLeverDomainException_QuandCommandePasEnAttente()
        {
            // Arrange : commande confirmée
            var commande = CreerCommandeConfirmee();
            var articleId = commande.Items.First().Id;

            // Act & Assert : retrait interdit après confirmation
            Assert.Throws<DomainException>(() => commande.RemoveItem(articleId));
        }

        [Fact]
        public void RetirerArticle_DoitLeverDomainException_QuandArticleIntrouvable()
        {
            // Arrange : commande en attente
            var commande = CreerCommandeAvecArticle();

            // Act & Assert : article avec un identifiant inexistant
            Assert.Throws<DomainException>(() => commande.RemoveItem(Guid.NewGuid()));
        }

        #endregion

        #region CalculerTotal

        [Fact]
        public void CalculerTotal_DoitRetournerSommeDesArticles()
        {
            // Arrange : commande avec 2 articles (2×10 + 3×5 = 35)
            var commande = new Order(_clientId);
            commande.AddItem(Guid.NewGuid(), 2, 10m);
            commande.AddItem(Guid.NewGuid(), 3, 5m);

            // Act : calculer le total
            var total = commande.CalculateTotal();

            // Assert : 20 + 15 = 35
            Assert.Equal(35m, total);
        }

        [Fact]
        public void CalculerTotal_DoitRetournerZero_QuandAucunArticle()
        {
            // Arrange : commande vide
            var commande = new Order(_clientId);

            // Act : calculer le total
            var total = commande.CalculateTotal();

            // Assert : total à zéro
            Assert.Equal(0m, total);
        }

        #endregion

        #region Transitions de statut — Confirmer

        [Fact]
        public void Confirmer_DoitPasserStatutAConfirme_QuandEnAttenteAvecArticles()
        {
            // Arrange : commande en attente avec un article
            var commande = CreerCommandeAvecArticle();

            // Act : confirmer la commande
            commande.Confirm();

            // Assert : statut passe à Confirmed
            Assert.Equal(OrderStatus.Confirmed, commande.Status);
        }

        [Fact]
        public void Confirmer_DoitLeverDomainException_QuandPasEnAttente()
        {
            // Arrange : commande déjà confirmée
            var commande = CreerCommandeConfirmee();

            // Act & Assert : double confirmation interdite
            Assert.Throws<DomainException>(() => commande.Confirm());
        }

        [Fact]
        public void Confirmer_DoitLeverDomainException_QuandAucunArticle()
        {
            // Arrange : commande vide (pas d'articles)
            var commande = new Order(_clientId);

            // Act & Assert : confirmation sans article interdite
            Assert.Throws<DomainException>(() => commande.Confirm());
        }

        #endregion

        #region Transitions de statut — Expédier

        [Fact]
        public void Expedier_DoitPasserStatutAExpedie_QuandConfirme()
        {
            // Arrange : commande confirmée
            var commande = CreerCommandeConfirmee();

            // Act : expédier
            commande.Ship();

            // Assert : statut passe à Shipped
            Assert.Equal(OrderStatus.Shipped, commande.Status);
        }

        [Fact]
        public void Expedier_DoitLeverDomainException_QuandPasConfirme()
        {
            // Arrange : commande en attente (pas encore confirmée)
            var commande = CreerCommandeAvecArticle();

            // Act & Assert : expédition impossible sans confirmation préalable
            Assert.Throws<DomainException>(() => commande.Ship());
        }

        #endregion

        #region Transitions de statut — Livrer

        [Fact]
        public void Livrer_DoitPasserStatutALivre_QuandExpedie()
        {
            // Arrange : commande expédiée
            var commande = CreerCommandeConfirmee();
            commande.Ship();

            // Act : livrer
            commande.Deliver();

            // Assert : statut passe à Delivered
            Assert.Equal(OrderStatus.Delivered, commande.Status);
        }

        [Fact]
        public void Livrer_DoitLeverDomainException_QuandPasExpedie()
        {
            // Arrange : commande confirmée (pas encore expédiée)
            var commande = CreerCommandeConfirmee();

            // Act & Assert : livraison impossible sans expédition préalable
            Assert.Throws<DomainException>(() => commande.Deliver());
        }

        #endregion

        #region Transitions de statut — Annuler

        [Fact]
        public void Annuler_DoitPasserStatutAAnnule_QuandEnAttente()
        {
            // Arrange : commande en attente
            var commande = new Order(_clientId);

            // Act : annuler
            commande.Cancel();

            // Assert : statut passe à Cancelled
            Assert.Equal(OrderStatus.Cancelled, commande.Status);
        }

        [Fact]
        public void Annuler_DoitPasserStatutAAnnule_QuandConfirme()
        {
            // Arrange : commande confirmée
            var commande = CreerCommandeConfirmee();

            // Act : annuler
            commande.Cancel();

            // Assert : statut passe à Cancelled
            Assert.Equal(OrderStatus.Cancelled, commande.Status);
        }

        [Fact]
        public void Annuler_DoitPasserStatutAAnnule_QuandExpedie()
        {
            // Arrange : commande expédiée
            var commande = CreerCommandeConfirmee();
            commande.Ship();

            // Act : annuler
            commande.Cancel();

            // Assert : statut passe à Cancelled
            Assert.Equal(OrderStatus.Cancelled, commande.Status);
        }

        [Fact]
        public void Annuler_DoitLeverDomainException_QuandDejaLivre()
        {
            // Arrange : commande livrée
            var commande = CreerCommandeConfirmee();
            commande.Ship();
            commande.Deliver();

            // Act & Assert : annulation impossible après livraison
            Assert.Throws<DomainException>(() => commande.Cancel());
        }

        [Fact]
        public void Annuler_DoitLeverDomainException_QuandDejaAnnule()
        {
            // Arrange : commande déjà annulée
            var commande = new Order(_clientId);
            commande.Cancel();

            // Act & Assert : double annulation interdite
            Assert.Throws<DomainException>(() => commande.Cancel());
        }

        #endregion

        #region Flux complet

        [Fact]
        public void FluxComplet_DoitTransiterCorrectement_DePendingALivre()
        {
            // Arrange : nouvelle commande
            var commande = new Order(_clientId);
            Assert.Equal(OrderStatus.Pending, commande.Status);

            // Ajouter des articles
            commande.AddItem(Guid.NewGuid(), 2, 25m);
            commande.AddItem(Guid.NewGuid(), 1, 50m);

            // Act & Assert : transition Pending → Confirmed
            commande.Confirm();
            Assert.Equal(OrderStatus.Confirmed, commande.Status);

            // Act & Assert : transition Confirmed → Shipped
            commande.Ship();
            Assert.Equal(OrderStatus.Shipped, commande.Status);

            // Act & Assert : transition Shipped → Delivered
            commande.Deliver();
            Assert.Equal(OrderStatus.Delivered, commande.Status);

            // Vérifier le total final : 2×25 + 1×50 = 100
            Assert.Equal(100m, commande.CalculateTotal());
        }

        #endregion
    }
}
