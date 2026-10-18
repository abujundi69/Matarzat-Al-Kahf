using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;

namespace MatarzatAlKahf.Application.Features.Customers.Interfaces;
public interface IChangePasswordCustomerProfileService
{
    Task ChangePasswordAsync(ChangeCustomerPasswordRequest request, CancellationToken cancellationToken);
}
