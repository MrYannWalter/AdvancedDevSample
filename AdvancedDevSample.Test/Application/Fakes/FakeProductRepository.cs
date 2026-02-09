using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedDevSample.Test.Application.Fakes
{
    public class FakeProductRepository : IProductRepository
    {
        public bool WasSaved { get; private set; }
        private readonly Product _product;
        public FakeProductRepository(Product product)
        {
            _product = product;
        }

        public Product GetById(Guid productId) => _product;

        public void Save(Product product)
        {
            WasSaved = true;
        }
    }
}
