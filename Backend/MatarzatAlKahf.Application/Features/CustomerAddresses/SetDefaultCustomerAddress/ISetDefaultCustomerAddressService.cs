using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAddresses.Interfaces;
public interface ISetDefaultCustomerAddressService
{
    Task SetDefaultAsync(Guid id, CancellationToken cancellationToken);
}
