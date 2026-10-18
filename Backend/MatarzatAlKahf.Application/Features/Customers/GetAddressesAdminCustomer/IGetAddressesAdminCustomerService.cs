using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Customers.Interfaces;
public interface IGetAddressesAdminCustomerService
{
    Task<IReadOnlyList<CustomerAddressResponse>> GetAddressesAsync(Guid id, CancellationToken cancellationToken);
}
