namespace AdvancedDevSample.Application.DTOs.Customer.CustomerResponses
{
    using AdvancedDevSample.Domain.Entities;

    public class CustomerResponse
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public bool IsActive { get; set; }

        public static CustomerResponse FromCustomer(Customer customer)
        {
            return new CustomerResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                IsActive = customer.IsActive
            };
        }
    }
}