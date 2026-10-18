using MatarzatAlKahf.Application.Features.Delivery.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Delivery.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Delivery.Interfaces;
public interface IUpdateDeliveryLocationService
{
    Task<AdminDeliveryLocationResponse> UpdateAsync(Guid id, SaveDeliveryLocationRequest request, CancellationToken cancellationToken);
}
