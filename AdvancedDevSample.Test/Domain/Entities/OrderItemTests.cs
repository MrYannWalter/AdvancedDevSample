using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Test.Domain.Entities
{
    /// <summary>
    /// Tests unitaires de l'entité OrderItem.
    /// Vérifie les règles métier : construction (quantité > 0, prix > 0),
    /// calcul du total, mise à jour de la quantité.
    /// </summary>
    public class OrderItemTests
    {
        #region Constructeur

        [Fact]
        public void Constructeur_DoitCreerArticle_QuandParametresValides()
        {
            // Arrange : identifiants et valeurs valides
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Act : création de l'article
            var article = new OrderItem(orderId, productId, 3, 25.50m);

            // Assert : propriétés correctement initialisées
            Assert.NotEqual(Guid.Empty, article.Id);
            Assert.Equal(orderId, article.OrderId);
            Assert.Equal(productId, article.ProductId);
            Assert.Equal(3, article.Quantity);
            Assert.Equal(25.50m, article.UnitPrice);
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandQuantiteEstZero()
        {
            // Act & Assert : quantité à zéro est interdite
            Assert.Throws<DomainException>(
                () => new OrderItem(Guid.NewGuid(), Guid.NewGuid(), 0, 10m));
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandQuantiteEstNegative()
        {
            // Act & Assert : quantité négative est interdite
            Assert.Throws<DomainException>(
                () => new OrderItem(Guid.NewGuid(), Guid.NewGuid(), -1, 10m));
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandPrixUnitaireEstZero()
        {
            // Act & Assert : prix unitaire à zéro est interdit
            Assert.Throws<DomainException>(
                () => new OrderItem(Guid.NewGuid(), Guid.NewGuid(), 2, 0m));
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandPrixUnitaireEstNegatif()
        {
            // Act & Assert : prix unitaire négatif est interdit
            Assert.Throws<DomainException>(
                () => new OrderItem(Guid.NewGuid(), Guid.NewGuid(), 2, -5m));
        }

        #endregion

        #region ObtenirTotal

        [Fact]
        public void ObtenirTotal_DoitRetournerQuantiteFoisPrixUnitaire()
        {
            // Arrange : article avec quantité 3 à 10€ l'unité
            var article = new OrderItem(Guid.NewGuid(), Guid.NewGuid(), 3, 10m);

            // Act : calculer le total
            var total = article.GetTotal();

            // Assert : 3 × 10 = 30
            Assert.Equal(30m, total);
        }

        #endregion

        #region MettreAJourQuantite

        [Fact]
        public void MettreAJourQuantite_DoitModifierQuantite_QuandValide()
        {
            // Arrange : article existant
            var article = new OrderItem(Guid.NewGuid(), Guid.NewGuid(), 2, 10m);

            // Act : mettre à jour la quantité
            article.UpdateQuantity(5);

            // Assert : la quantité est mise à jour
            Assert.Equal(5, article.Quantity);
        }

        [Fact]
        public void MettreAJourQuantite_DoitLeverDomainException_QuandQuantiteEstZero()
        {
            // Arrange : article existant
            var article = new OrderItem(Guid.NewGuid(), Guid.NewGuid(), 2, 10m);

            // Act & Assert : quantité zéro rejetée
            Assert.Throws<DomainException>(() => article.UpdateQuantity(0));
        }

        [Fact]
        public void MettreAJourQuantite_DoitLeverDomainException_QuandQuantiteEstNegative()
        {
            // Arrange : article existant
            var article = new OrderItem(Guid.NewGuid(), Guid.NewGuid(), 2, 10m);

            // Act & Assert : quantité négative rejetée
            Assert.Throws<DomainException>(() => article.UpdateQuantity(-1));
        }

        #endregion
    }
}
