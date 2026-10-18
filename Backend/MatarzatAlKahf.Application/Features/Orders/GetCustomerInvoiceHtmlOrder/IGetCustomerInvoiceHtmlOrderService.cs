using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Orders.Interfaces;
public interface IGetCustomerInvoiceHtmlOrderService
{
    Task<string> GetCustomerInvoiceHtmlAsync(Guid id, CancellationToken cancellationToken);
}
