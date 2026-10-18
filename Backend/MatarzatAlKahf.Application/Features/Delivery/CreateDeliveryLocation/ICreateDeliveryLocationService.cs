using MatarzatAlKahf.Application.Features.Delivery.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Delivery.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Delivery.Interfaces;
public interface ICreateDeliveryLocationService
{
    Task<AdminDeliveryLocationResponse> CreateAsync(SaveDeliveryLocationRequest request, CancellationToken cancellationToken);
}
