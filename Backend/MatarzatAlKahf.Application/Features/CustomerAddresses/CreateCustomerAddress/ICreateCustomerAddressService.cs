using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAddresses.Interfaces;
public interface ICreateCustomerAddressService
{
    Task<CustomerAddressResponse> CreateAsync(SaveCustomerAddressRequest request, CancellationToken cancellationToken);
}
