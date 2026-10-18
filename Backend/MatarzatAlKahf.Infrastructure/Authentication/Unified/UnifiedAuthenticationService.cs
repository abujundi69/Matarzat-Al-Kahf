using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Authentication.Unified;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Authentication.Unified;

public sealed class UnifiedAuthenticationService(
    MatarzatAlKahfDbContext dbContext,
    IPhoneNumberNormalizer phoneNumberNormalizer,
    IAdminAuthenticationService adminAuthenticationService,
    ILoginCustomerAuthenticationService customerAuthenticationService)
    : IUnifiedAuthenticationService
{
    private const string InvalidCredentialsMessage = "Invalid login credentials.";
    private const string CustomerRole = "Customer";

    public async Task<UnifiedLoginResponse> LoginAsync(
        UnifiedLoginRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.PhoneNumber)
            || string.IsNullOrEmpty(request.Password))
        {
            throw new AuthenticationFailedException(InvalidCredentialsMessage);
        }

        string normalizedPhoneNumber;
        try
        {
            normalizedPhoneNumber = phoneNumberNormalizer.Normalize(request.PhoneNumber);
        }
        catch (ValidationException)
        {
            // Login deliberately returns one generic response for malformed and unknown
            // identifiers so the unified endpoint cannot be used for account discovery.
            throw new AuthenticationFailedException(InvalidCredentialsMessage);
        }

        bool adminExists = await dbContext.AdminUsers
            .AsNoTracking()
            .AnyAsync(
                candidate => candidate.NormalizedPhoneNumber == normalizedPhoneNumber,
                cancellationToken);
        bool customerExists = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(
                candidate => candidate.NormalizedPhoneNumber == normalizedPhoneNumber,
                cancellationToken);

        try
        {
            if (adminExists && customerExists)
            {
                return await LoginSharedPhoneAsync(request, cancellationToken);
            }

            if (adminExists)
            {
                return await LoginAdminAsync(request, cancellationToken);
            }

            if (customerExists)
            {
                return await LoginCustomerAsync(request, cancellationToken);
            }
        }
        catch (AuthenticationFailedException)
        {
            // Normalize the two existing services' credential error messages into a
            // single contract so identity type cannot be inferred from the response.
            throw new AuthenticationFailedException(InvalidCredentialsMessage);
        }

        throw new AuthenticationFailedException(InvalidCredentialsMessage);
    }

    private async Task<UnifiedLoginResponse> LoginSharedPhoneAsync(
        UnifiedLoginRequest request,
        CancellationToken cancellationToken)
    {
        AuthorizationDeniedException? adminDenied = null;

        try
        {
            // When credentials are valid in both stores, administrative access wins so
            // an administrator is routed to the permission-controlled application area.
            return await LoginAdminAsync(request, cancellationToken);
        }
        catch (AuthenticationFailedException)
        {
            // The password may belong to the customer record with the same phone.
        }
        catch (AuthorizationDeniedException exception)
        {
            // Still try an active customer record. If it does not authenticate, retain
            // the matched administrator's established inactive-account response.
            adminDenied = exception;
        }

        try
        {
            return await LoginCustomerAsync(request, cancellationToken);
        }
        catch (AuthenticationFailedException) when (adminDenied is not null)
        {
            throw new AuthorizationDeniedException(adminDenied.Message);
        }
        catch (AuthorizationDeniedException) when (adminDenied is not null)
        {
            throw new AuthorizationDeniedException(adminDenied.Message);
        }
    }

    private async Task<UnifiedLoginResponse> LoginAdminAsync(
        UnifiedLoginRequest request,
        CancellationToken cancellationToken)
    {
        AdminLoginResponse response = await adminAuthenticationService.LoginAsync(
            new AdminLoginRequest(request.PhoneNumber, request.Password),
            cancellationToken);
        AdminProfileDto profile = response.Admin;

        return new UnifiedLoginResponse(
            response.AccessToken,
            response.TokenType,
            response.ExpiresAtUtc,
            new AuthenticatedActorDto(
                profile.Id,
                profile.FullName,
                profile.PhoneNumber,
                profile.Email,
                profile.PreferredLanguage,
                AuthenticationActorTypes.Admin,
                profile.Roles,
                profile.Permissions));
    }

    private async Task<UnifiedLoginResponse> LoginCustomerAsync(
        UnifiedLoginRequest request,
        CancellationToken cancellationToken)
    {
        CustomerLoginResponse response = await customerAuthenticationService.LoginAsync(
            new CustomerLoginRequest(request.PhoneNumber, request.Password),
            cancellationToken);
        CustomerProfileResponse profile = response.Customer;

        return new UnifiedLoginResponse(
            response.AccessToken,
            response.TokenType,
            response.ExpiresAtUtc,
            new AuthenticatedActorDto(
                profile.Id,
                profile.FullName,
                profile.PhoneNumber,
                profile.Email,
                profile.PreferredLanguage,
                AuthenticationActorTypes.Customer,
                [CustomerRole],
                []));
    }
}
