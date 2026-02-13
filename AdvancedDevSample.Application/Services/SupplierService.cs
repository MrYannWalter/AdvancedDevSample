using AdvancedDevSample.Application.DTOs.Supplier.SupplierRequests;
using AdvancedDevSample.Application.Exceptions;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using System.Net;

namespace AdvancedDevSample.Application.Services
{
    public class SupplierService
    {
        private readonly ISupplierRepository _repository;

        public SupplierService(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Supplier> GetAllSuppliers()
        {
            return _repository.ListAll();
        }

        public Supplier GetSupplier(Guid supplierId)
        {
            return _repository.GetById(supplierId)
                ?? throw new ApplicationServiceException("Fournisseur introuvable.", HttpStatusCode.NotFound);
        }

        public Supplier CreateSupplier(CreateSupplierRequest request)
        {
            var supplier = new Supplier(request.CompanyName, request.ContactEmail, request.Phone);
            _repository.Add(supplier);
            return supplier;
        }

        public void UpdateSupplier(Guid supplierId, UpdateSupplierRequest request)
        {
            var supplier = GetSupplier(supplierId);
            supplier.UpdateInfo(request.CompanyName, request.ContactEmail, request.Phone);
            _repository.Save(supplier);
        }

        public void DeleteSupplier(Guid supplierId)
        {
            var supplier = GetSupplier(supplierId);
            _repository.Remove(supplierId);
        }

        public void ActivateSupplier(Guid supplierId)
        {
            var supplier = GetSupplier(supplierId);
            supplier.Activate();
            _repository.Save(supplier);
        }

        public void DeactivateSupplier(Guid supplierId)
        {
            var supplier = GetSupplier(supplierId);
            supplier.Deactivate();
            _repository.Save(supplier);
        }
    }
}
