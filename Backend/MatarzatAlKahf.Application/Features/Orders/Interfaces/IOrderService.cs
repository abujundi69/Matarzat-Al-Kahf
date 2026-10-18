using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Orders.Interfaces;
public interface IOrderService : IListCustomerOrderService, IGetCustomerOrderService, IGetCustomerInvoiceHtmlOrderService, ICancelCustomerOrderService, ILookupGuestOrderService, IListAdminOrderService, IGetAdminOrderService, IChangeStatusOrderService, IUpdateNotesOrderService
{
}
