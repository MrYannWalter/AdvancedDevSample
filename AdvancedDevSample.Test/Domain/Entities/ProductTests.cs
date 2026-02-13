using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Test.Domain.Entities
{
    /// <summary>
    /// Tests unitaires de l'entité Product.
    /// Vérifie les règles métier : construction, changement de prix,
    /// mise à jour, remise, activation/désactivation.
    /// </summary>
    public class ProductTests
    {
        #region Constructeur

        [Fact]
        public void Constructeur_DoitCreerProduit_QuandParametresValides()
        {
            // Arrange : paramètres valides
            var nom = "Clavier mécanique";
            var description = "Clavier RGB avec switches bleus";
            var prix = 89.99m;

            // Act : création du produit
            var produit = new Product(nom, description, prix);

            // Assert : les propriétés sont correctement initialisées
            Assert.NotEqual(Guid.Empty, produit.Id);
            Assert.Equal(nom, produit.Name);
            Assert.Equal(description, produit.Description);
            Assert.Equal(prix, produit.Price);
            Assert.True(produit.IsActive);
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandNomEstNull()
        {
            // Act & Assert : un nom null déclenche une DomainException
            var exception = Assert.Throws<DomainException>(
                () => new Product(null!, "description", 10m));

            Assert.Contains("nom", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandDescriptionEstNull()
        {
            // Act & Assert : une description null déclenche une DomainException
            var exception = Assert.Throws<DomainException>(
                () => new Product("Produit", null!, 10m));

            Assert.Contains("description", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandPrixEstZero()
        {
            // Act & Assert : un prix à zéro est interdit
            Assert.Throws<DomainException>(
                () => new Product("Produit", "Description", 0m));
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandPrixEstNegatif()
        {
            // Act & Assert : un prix négatif est interdit
            Assert.Throws<DomainException>(
                () => new Product("Produit", "Description", -5m));
        }

        #endregion

        #region ChangerPrix

        [Fact]
        public void ChangerPrix_DoitMettreAJourLePrix_QuandProduitActifEtPrixValide()
        {
            // Arrange : produit actif avec prix initial
            var produit = new Product("Produit", "Description", 50m);

            // Act : changer le prix à 75
            produit.ChangePrice(75m);

            // Assert : le prix est mis à jour
            Assert.Equal(75m, produit.Price);
        }

        [Fact]
        public void ChangerPrix_DoitLeverDomainException_QuandProduitEstInactif()
        {
            // Arrange : désactiver le produit
            var produit = new Product("Produit", "Description", 50m);
            produit.Deactivate();

            // Act & Assert : changer le prix sur un produit inactif lève une exception
            Assert.Throws<DomainException>(() => produit.ChangePrice(75m));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void ChangerPrix_DoitLeverDomainException_QuandPrixEstZeroOuNegatif(decimal prixInvalide)
        {
            // Arrange : produit actif
            var produit = new Product("Produit", "Description", 50m);

            // Act & Assert : un prix <= 0 est rejeté
            Assert.Throws<DomainException>(() => produit.ChangePrice(prixInvalide));
        }

        #endregion

        #region MettreAJourInfo

        [Fact]
        public void MettreAJourInfo_DoitModifierNomEtDescription_QuandValides()
        {
            // Arrange : produit existant
            var produit = new Product("Ancien nom", "Ancienne description", 10m);

            // Act : mettre à jour les informations
            produit.UpdateInfo("Nouveau nom", "Nouvelle description");

            // Assert : les champs sont mis à jour
            Assert.Equal("Nouveau nom", produit.Name);
            Assert.Equal("Nouvelle description", produit.Description);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MettreAJourInfo_DoitLeverDomainException_QuandNomEstVideOuNull(string? nomInvalide)
        {
            // Arrange : produit existant
            var produit = new Product("Produit", "Description", 10m);

            // Act & Assert : un nom vide ou null est rejeté
            Assert.Throws<DomainException>(() => produit.UpdateInfo(nomInvalide!, "Description valide"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MettreAJourInfo_DoitLeverDomainException_QuandDescriptionEstVideOuNull(string? descriptionInvalide)
        {
            // Arrange : produit existant
            var produit = new Product("Produit", "Description", 10m);

            // Act & Assert : une description vide ou null est rejetée
            Assert.Throws<DomainException>(() => produit.UpdateInfo("Nom valide", descriptionInvalide!));
        }

        #endregion

        #region AppliquerRemise

        [Fact]
        public void AppliquerRemise_DoitReduireLePrix_QuandPourcentageValide()
        {
            // Arrange : produit actif à 100€
            var produit = new Product("Produit", "Description", 100m);

            // Act : appliquer 10% de remise
            produit.ApplyDiscount(10m);

            // Assert : le prix passe à 90€ (100 - 10%)
            Assert.Equal(90m, produit.Price);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(100)]
        [InlineData(150)]
        public void AppliquerRemise_DoitLeverDomainException_QuandPourcentageInvalide(decimal pourcentage)
        {
            // Arrange : produit actif
            var produit = new Product("Produit", "Description", 100m);

            // Act & Assert : pourcentage hors ]0, 100[ est rejeté
            Assert.Throws<DomainException>(() => produit.ApplyDiscount(pourcentage));
        }

        [Fact]
        public void AppliquerRemise_DoitLeverDomainException_QuandProduitEstInactif()
        {
            // Arrange : produit désactivé
            var produit = new Product("Produit", "Description", 100m);
            produit.Deactivate();

            // Act & Assert : impossible d'appliquer une remise sur un produit inactif
            Assert.Throws<DomainException>(() => produit.ApplyDiscount(10m));
        }

        #endregion

        #region Activation / Désactivation

        [Fact]
        public void Desactiver_DoitRendreProduitInactif()
        {
            // Arrange : produit actif par défaut
            var produit = new Product("Produit", "Description", 10m);

            // Act : désactivation
            produit.Deactivate();

            // Assert : le produit est inactif
            Assert.False(produit.IsActive);
        }

        [Fact]
        public void Activer_DoitRendreProduitActif_ApresDesactivation()
        {
            // Arrange : produit désactivé
            var produit = new Product("Produit", "Description", 10m);
            produit.Deactivate();

            // Act : réactivation
            produit.Activate();

            // Assert : le produit est de nouveau actif
            Assert.True(produit.IsActive);
        }

        #endregion
    }
}
