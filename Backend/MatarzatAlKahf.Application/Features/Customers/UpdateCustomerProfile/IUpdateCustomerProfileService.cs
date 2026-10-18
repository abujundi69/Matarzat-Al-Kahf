using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;

namespace MatarzatAlKahf.Application.Features.Customers.Interfaces;
public interface IUpdateCustomerProfileService
{
    Task<CustomerProfileResponse> UpdateAsync(UpdateCustomerProfileRequest request, CancellationToken cancellationToken);
}
