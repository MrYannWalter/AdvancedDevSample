using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Test.Domain.Entities
{
    /// <summary>
    /// Tests unitaires de l'entité Supplier.
    /// Vérifie les règles métier : construction (avec téléphone optionnel),
    /// mise à jour, activation/désactivation, unicité des identifiants.
    /// </summary>
    public class SupplierTests
    {
        #region Constructeur

        [Fact]
        public void Constructeur_DoitCreerFournisseur_QuandParametresValides()
        {
            // Arrange & Act : création avec tous les paramètres
            var fournisseur = new Supplier("Acme Corp", "contact@acme.com", "0123456789");

            // Assert : propriétés correctement initialisées
            Assert.NotEqual(Guid.Empty, fournisseur.Id);
            Assert.Equal("Acme Corp", fournisseur.CompanyName);
            Assert.Equal("contact@acme.com", fournisseur.ContactEmail);
            Assert.Equal("0123456789", fournisseur.Phone);
            Assert.True(fournisseur.IsActive);
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandNomSocieteEstNull()
        {
            // Act & Assert : nom de société null déclenche une exception
            Assert.Throws<DomainException>(
                () => new Supplier(null!, "email@test.com", "0123456789"));
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandEmailContactEstNull()
        {
            // Act & Assert : email de contact null déclenche une exception
            Assert.Throws<DomainException>(
                () => new Supplier("Acme", null!, "0123456789"));
        }

        [Fact]
        public void Constructeur_DoitAccepterTelephoneNull_EtLeRemplacerParChaineVide()
        {
            // Arrange & Act : téléphone null est accepté
            var fournisseur = new Supplier("Acme", "email@test.com", null!);

            // Assert : le téléphone est remplacé par une chaîne vide
            Assert.Equal(string.Empty, fournisseur.Phone);
        }

        [Fact]
        public void Constructeur_DoitGenererUnIdUnique()
        {
            // Arrange & Act : créer deux fournisseurs
            var f1 = new Supplier("Société A", "a@test.com", "111");
            var f2 = new Supplier("Société B", "b@test.com", "222");

            // Assert : identifiants différents
            Assert.NotEqual(f1.Id, f2.Id);
        }

        [Fact]
        public void Constructeur_DoitInitialiserFournisseurActifParDefaut()
        {
            // Act : création du fournisseur
            var fournisseur = new Supplier("Acme", "email@test.com", "0123");

            // Assert : actif par défaut
            Assert.True(fournisseur.IsActive);
        }

        #endregion

        #region MettreAJourInfo

        [Fact]
        public void MettreAJourInfo_DoitModifierLesDonnees_QuandParametresValides()
        {
            // Arrange : fournisseur existant
            var fournisseur = new Supplier("Ancien nom", "ancien@test.com", "000");

            // Act : mise à jour
            fournisseur.UpdateInfo("Nouveau nom", "nouveau@test.com", "999");

            // Assert : les champs sont mis à jour
            Assert.Equal("Nouveau nom", fournisseur.CompanyName);
            Assert.Equal("nouveau@test.com", fournisseur.ContactEmail);
            Assert.Equal("999", fournisseur.Phone);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MettreAJourInfo_DoitLeverDomainException_QuandNomSocieteEstVideOuNull(string? nomInvalide)
        {
            // Arrange : fournisseur existant
            var fournisseur = new Supplier("Acme", "email@test.com", "0123");

            // Act & Assert : nom de société vide/null rejeté
            Assert.Throws<DomainException>(
                () => fournisseur.UpdateInfo(nomInvalide!, "email@test.com", "0123"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MettreAJourInfo_DoitLeverDomainException_QuandEmailContactEstVideOuNull(string? emailInvalide)
        {
            // Arrange : fournisseur existant
            var fournisseur = new Supplier("Acme", "email@test.com", "0123");

            // Act & Assert : email vide/null rejeté
            Assert.Throws<DomainException>(
                () => fournisseur.UpdateInfo("Acme", emailInvalide!, "0123"));
        }

        [Fact]
        public void MettreAJourInfo_DoitAccepterTelephoneNull_SansException()
        {
            // Arrange : fournisseur existant avec un téléphone
            var fournisseur = new Supplier("Acme", "email@test.com", "0123");

            // Act : mise à jour avec téléphone null (optionnel)
            fournisseur.UpdateInfo("Acme", "email@test.com", null!);

            // Assert : le téléphone est remplacé par chaîne vide, pas d'exception
            Assert.Equal(string.Empty, fournisseur.Phone);
        }

        #endregion

        #region Activation / Désactivation

        [Fact]
        public void Desactiver_DoitRendreFournisseurInactif()
        {
            // Arrange : fournisseur actif
            var fournisseur = new Supplier("Acme", "email@test.com", "0123");

            // Act : désactivation
            fournisseur.Deactivate();

            // Assert : inactif
            Assert.False(fournisseur.IsActive);
        }

        [Fact]
        public void Activer_DoitRendreFournisseurActif_ApresDesactivation()
        {
            // Arrange : fournisseur désactivé
            var fournisseur = new Supplier("Acme", "email@test.com", "0123");
            fournisseur.Deactivate();

            // Act : réactivation
            fournisseur.Activate();

            // Assert : de nouveau actif
            Assert.True(fournisseur.IsActive);
        }

        #endregion
    }
}
