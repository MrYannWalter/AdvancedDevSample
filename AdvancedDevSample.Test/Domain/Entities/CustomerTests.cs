using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Test.Domain.Entities
{
    /// <summary>
    /// Tests unitaires de l'entité Customer.
    /// Vérifie les règles métier : construction, mise à jour,
    /// activation/désactivation, unicité des identifiants.
    /// </summary>
    public class CustomerTests
    {
        #region Constructeur

        [Fact]
        public void Constructeur_DoitCreerClient_QuandParametresValides()
        {
            // Arrange & Act : création avec des paramètres valides
            var client = new Customer("Jean", "Dupont", "jean@example.com");

            // Assert : toutes les propriétés sont correctement initialisées
            Assert.NotEqual(Guid.Empty, client.Id);
            Assert.Equal("Jean", client.FirstName);
            Assert.Equal("Dupont", client.LastName);
            Assert.Equal("jean@example.com", client.Email);
            Assert.True(client.IsActive);
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandPrenomEstNull()
        {
            // Act & Assert : prénom null déclenche une exception
            Assert.Throws<DomainException>(
                () => new Customer(null!, "Dupont", "email@test.com"));
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandNomEstNull()
        {
            // Act & Assert : nom null déclenche une exception
            Assert.Throws<DomainException>(
                () => new Customer("Jean", null!, "email@test.com"));
        }

        [Fact]
        public void Constructeur_DoitLeverDomainException_QuandEmailEstNull()
        {
            // Act & Assert : email null déclenche une exception
            Assert.Throws<DomainException>(
                () => new Customer("Jean", "Dupont", null!));
        }

        [Fact]
        public void Constructeur_DoitGenererUnIdUnique()
        {
            // Arrange & Act : créer deux clients
            var client1 = new Customer("Jean", "Dupont", "jean@test.com");
            var client2 = new Customer("Marie", "Martin", "marie@test.com");

            // Assert : les identifiants sont différents
            Assert.NotEqual(client1.Id, client2.Id);
        }

        #endregion

        #region MettreAJourInfo

        [Fact]
        public void MettreAJourInfo_DoitModifierLesDonnees_QuandParametresValides()
        {
            // Arrange : client existant
            var client = new Customer("Jean", "Dupont", "jean@test.com");

            // Act : mise à jour des informations
            client.UpdateInfo("Pierre", "Martin", "pierre@test.com");

            // Assert : les champs sont mis à jour
            Assert.Equal("Pierre", client.FirstName);
            Assert.Equal("Martin", client.LastName);
            Assert.Equal("pierre@test.com", client.Email);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MettreAJourInfo_DoitLeverDomainException_QuandPrenomEstVideOuNull(string? prenomInvalide)
        {
            // Arrange : client existant
            var client = new Customer("Jean", "Dupont", "jean@test.com");

            // Act & Assert : prénom vide/null rejeté
            Assert.Throws<DomainException>(
                () => client.UpdateInfo(prenomInvalide!, "Dupont", "jean@test.com"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MettreAJourInfo_DoitLeverDomainException_QuandNomEstVideOuNull(string? nomInvalide)
        {
            // Arrange : client existant
            var client = new Customer("Jean", "Dupont", "jean@test.com");

            // Act & Assert : nom vide/null rejeté
            Assert.Throws<DomainException>(
                () => client.UpdateInfo("Jean", nomInvalide!, "jean@test.com"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MettreAJourInfo_DoitLeverDomainException_QuandEmailEstVideOuNull(string? emailInvalide)
        {
            // Arrange : client existant
            var client = new Customer("Jean", "Dupont", "jean@test.com");

            // Act & Assert : email vide/null rejeté
            Assert.Throws<DomainException>(
                () => client.UpdateInfo("Jean", "Dupont", emailInvalide!));
        }

        #endregion

        #region Activation / Désactivation

        [Fact]
        public void Desactiver_DoitRendreClientInactif()
        {
            // Arrange : client actif par défaut
            var client = new Customer("Jean", "Dupont", "jean@test.com");

            // Act : désactivation
            client.Deactivate();

            // Assert : le client est inactif
            Assert.False(client.IsActive);
        }

        [Fact]
        public void Activer_DoitRendreClientActif_ApresDesactivation()
        {
            // Arrange : client désactivé
            var client = new Customer("Jean", "Dupont", "jean@test.com");
            client.Deactivate();

            // Act : réactivation
            client.Activate();

            // Assert : le client est de nouveau actif
            Assert.True(client.IsActive);
        }

        #endregion
    }
}
