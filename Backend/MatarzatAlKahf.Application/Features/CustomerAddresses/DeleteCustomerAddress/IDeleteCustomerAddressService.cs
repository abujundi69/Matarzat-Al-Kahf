using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAddresses.Interfaces;
public interface IDeleteCustomerAddressService
{
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
