using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAddresses.Interfaces;
public interface IGetCustomerAddressService
{
    Task<CustomerAddressResponse> GetAsync(Guid id, CancellationToken cancellationToken);
}
