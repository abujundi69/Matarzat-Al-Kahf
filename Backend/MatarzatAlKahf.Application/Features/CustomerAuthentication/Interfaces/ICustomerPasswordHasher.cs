using MatarzatAlKahf.Domain.Entities.Customers;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;

public interface ICustomerPasswordHasher
{
    string HashPassword(Customer customer, string password);

    bool VerifyPassword(Customer customer, string password);
}
