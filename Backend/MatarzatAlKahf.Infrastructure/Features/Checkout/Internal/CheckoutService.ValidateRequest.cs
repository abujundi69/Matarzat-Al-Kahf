using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Checkout.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Checkout.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Checkout.Interfaces;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Checkout.Services;
public sealed partial class CheckoutService
{
    private void ValidateRequest(CheckoutRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (request.DeliveryLocationId == Guid.Empty)
        {
            errors[nameof(request.DeliveryLocationId)] = ["Delivery location is required."];
        }

        if (request.CustomerAddressId == Guid.Empty)
        {
            errors[nameof(request.CustomerAddressId)] = ["Customer address is invalid."];
        }
        else if (!currentActor.CustomerId.HasValue && request.CustomerAddressId.HasValue)
        {
            errors[nameof(request.CustomerAddressId)] = ["A saved address can only be used by an authenticated customer."];
        }

        bool useSavedAddress = currentActor.CustomerId.HasValue
            && request.CustomerAddressId.HasValue
            && request.CustomerAddressId != Guid.Empty;
        if (!useSavedAddress)
        {
            Check(request.CustomerName, 200, nameof(request.CustomerName), errors);
            Check(request.PhoneNumber, 30, nameof(request.PhoneNumber), errors);
            Check(request.Area, 150, nameof(request.Area), errors);
            Check(request.AddressLine, 1000, nameof(request.AddressLine), errors);
        }

        if (!currentActor.CustomerId.HasValue
            && string.IsNullOrWhiteSpace(request.GuestCartToken))
        {
            errors[nameof(request.GuestCartToken)] = ["Guest cart token is required."];
        }

        if (!Enum.IsDefined(request.PaymentMethod))
        {
            errors[nameof(request.PaymentMethod)] = ["Payment method is invalid."];
        }

        if (!Enum.IsDefined(request.Language))
        {
            errors[nameof(request.Language)] = ["Language is invalid."];
        }

        if (!useSavedAddress && request.TaxNumber?.Length > 100)
        {
            errors[nameof(request.TaxNumber)] = ["Tax number cannot exceed 100 characters."];
        }

        if (!useSavedAddress && request.DeliveryNotes?.Length > 2000)
        {
            errors[nameof(request.DeliveryNotes)] = ["Delivery notes cannot exceed 2000 characters."];
        }

        if (request.PaymentReference?.Trim().Length > 250)
        {
            errors[nameof(request.PaymentReference)] = ["Payment reference cannot exceed 250 characters."];
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentReference)
            && request.PaymentMethod is not (PaymentMethod.BankTransfer or PaymentMethod.ElectronicWallet))
        {
            errors[nameof(request.PaymentReference)] = ["A payment reference is only accepted for a bank transfer or electronic wallet payment."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Checkout data is invalid.", errors);
        }
    }
}
