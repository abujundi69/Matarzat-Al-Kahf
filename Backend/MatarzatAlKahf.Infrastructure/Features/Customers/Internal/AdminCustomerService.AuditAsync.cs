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
public sealed partial class AdminCustomerService
{
    private Task AuditAsync(string action, Customer customer, CancellationToken cancellationToken) => auditService.RecordAsync(action, nameof(Customer), customer.Id, $"Customer '{customer.FullName}' was changed.", cancellationToken);
}
