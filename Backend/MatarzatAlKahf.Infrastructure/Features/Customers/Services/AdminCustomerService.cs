using System.Linq.Expressions;
using System.Net.Mail;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.Interfaces;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Customers.Services;
public sealed partial class AdminCustomerService(MatarzatAlKahfDbContext dbContext, IAuditService auditService) : IAdminCustomerService
{
    private static Expression<Func<Customer, AdminCustomerResponse>> CustomerProjection { get; } = customer => new(customer.Id, customer.FullName, customer.PhoneNumber, customer.Email == string.Empty ? null : customer.Email, customer.Status, customer.IsPhoneVerified, customer.PreferredLanguage, customer.AdministrativeNotes == string.Empty ? null : customer.AdministrativeNotes, customer.Addresses.Count(address => !address.IsDeleted), customer.Orders.Count(order => !order.IsDeleted), customer.Orders.Where(order => !order.IsDeleted && order.Status == OrderStatus.Delivered).Sum(order => order.TotalAmount), customer.LastLoginAtUtc, customer.CreatedAtUtc);
}
