using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAddresses.Interfaces;
public interface IUpdateCustomerAddressService
{
    Task<CustomerAddressResponse> UpdateAsync(Guid id, SaveCustomerAddressRequest request, CancellationToken cancellationToken);
}
