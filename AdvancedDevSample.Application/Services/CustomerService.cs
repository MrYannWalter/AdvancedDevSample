using AdvancedDevSample.Application.DTOs.Customer.CustomerRequests;
using AdvancedDevSample.Application.Exceptions;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using System.Net;

namespace AdvancedDevSample.Application.Services
{
    public class CustomerService
    {
        private readonly ICustomerRepository _repository;

        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            return _repository.ListAll();
        }

        public Customer GetCustomer(Guid customerId)
        {
            return _repository.GetById(customerId)
                ?? throw new ApplicationServiceException("Client introuvable.", HttpStatusCode.NotFound);
        }

        public Customer CreateCustomer(CreateCustomerRequest request)
        {
            var customer = new Customer(request.FirstName, request.LastName, request.Email);
            _repository.Add(customer);
            return customer;
        }

        public void UpdateCustomer(Guid customerId, UpdateCustomerRequest request)
        {
            var customer = GetCustomer(customerId);
            customer.UpdateInfo(request.FirstName, request.LastName, request.Email);
            _repository.Save(customer);
        }

        public void DeleteCustomer(Guid customerId)
        {
            var customer = GetCustomer(customerId);
            _repository.Remove(customerId);
        }

        public void DeactivateCustomer(Guid customerId)
        {
            var customer = GetCustomer(customerId);
            customer.Deactivate();
            _repository.Save(customer);
        }

        public void ActivateCustomer(Guid customerId)
        {
            var customer = GetCustomer(customerId);
            customer.Activate();
            _repository.Save(customer);
        }
    }
}
