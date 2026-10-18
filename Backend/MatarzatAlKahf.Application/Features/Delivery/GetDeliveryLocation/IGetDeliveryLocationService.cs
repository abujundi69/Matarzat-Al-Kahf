using MatarzatAlKahf.Application.Features.Delivery.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Delivery.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Delivery.Interfaces;
public interface IGetDeliveryLocationService
{
    Task<AdminDeliveryLocationResponse> GetAsync(Guid id, CancellationToken cancellationToken);
}
