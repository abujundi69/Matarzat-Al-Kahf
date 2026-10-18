using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Domain.Entities.Customers;
using Microsoft.AspNetCore.Identity;

namespace MatarzatAlKahf.Infrastructure.Features.CustomerAuthentication.Services;

public sealed class CustomerPasswordHasher : ICustomerPasswordHasher
{
    private readonly PasswordHasher<Customer> _hasher = new();

    public string HashPassword(Customer customer, string password) =>
        _hasher.HashPassword(customer, password);

    public bool VerifyPassword(Customer customer, string password) =>
        _hasher.VerifyHashedPassword(customer, customer.PasswordHash, password)
        != PasswordVerificationResult.Failed;
}
