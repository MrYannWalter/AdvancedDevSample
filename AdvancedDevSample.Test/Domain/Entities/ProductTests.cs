using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedDevSample.Test.Domain.Entities
{
    public class ProductTests
    {
        [Fact]
        public void ChangePrice_Should_Update_Price_When_Product_Is_Active()
        {
            //Arrange : Je prepare un produit valide
            var product = new Product();
            product.ChangePrice(10);//valuer initial

            //Act : execute une action
            product.ChangePrice(20);

            //Assert : verification
            Assert.Equal(20, product.Price);
        }

        [Fact]
        public void ChangePrice_Should_Throw_Exception_When_Product_Is_Active()
        {
            //Arrange : je prepare un produit valide
            var product = new Product();
            product.ChangePrice(10);//valeur initiale


            //Simulation : produit désactiver(via reconstitution ou méthode dédiée)
            //product.IsActive = true; //Accesseur non accessible
            typeof(Product).GetProperty(nameof(Product.IsActive))!.SetValue(product, false);

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => product.ChangePrice(30));

            Assert.Equal("Produit inactif", exception.Message);
        }

        [Fact]
        public void ApplyDiscount_Should_Decrease_Price()
        {
            //Arrange
            var product = new Product();
            product.ChangePrice(50);//valeur initial

            // Act
            product.ApplyDiscount(10);

            //Assert
            Assert.Equal(40, product.Price);
        }
        [Fact]
        public void ApplyDiscount_Should_Throw_Exception_When_Resulting_Price_Is_Invalide()
        {
            //Arrange
            var product = new Product();
            product.ChangePrice(50);// Initial value

            // Act & Assert
            Assert.Throws<DomainException>(() => product.ApplyDiscount(100));
        }
    }
}
