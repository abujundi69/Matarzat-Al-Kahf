using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MatarzatAlKahf.Api.Errors;
using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MatarzatAlKahf.Api.Authentication;

internal static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddApiJwtBearer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        JwtSettings settings = JwtSettings.FromConfiguration(configuration);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));

        services.AddSingleton(settings);
        services.AddSingleton<IAdminTokenService, JwtAdminTokenService>();
        services.AddSingleton<ICustomerTokenService, JwtCustomerTokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.SaveToken = false;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        string? customerId = context.Principal?.FindFirstValue(
                            CustomerClaimTypes.CustomerId);
                        string? adminUserId = context.Principal?.FindFirstValue(
                            AdminClaimTypes.AdminUserId);
                        string? tokenId = context.Principal?.FindFirstValue(
                            System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti);
                        if (tokenId is null || (customerId is null && adminUserId is null))
                        {
                            return;
                        }

                        string hash = Convert.ToHexString(
                            SHA256.HashData(Encoding.UTF8.GetBytes(tokenId)));
                        var dbContext = context.HttpContext.RequestServices
                            .GetRequiredService<MatarzatAlKahfDbContext>();

                        if (adminUserId is not null)
                        {
                            // Administrative access is revocable: logout, suspension,
                            // deletion, and password resets clear the session row, and the
                            // token stops being accepted on the next request.
                            bool adminActive = await dbContext.AdminSessions
                                .AsNoTracking()
                                .AnyAsync(session =>
                                    session.TokenIdHash == hash
                                    && session.RevokedAtUtc == null
                                    && session.ExpiresAtUtc > DateTime.UtcNow
                                    && !session.AdminUser.IsDeleted
                                    && session.AdminUser.Status == UserStatus.Active);
                            if (!adminActive)
                            {
                                context.Fail("Administrative session is no longer active.");
                            }

                            return;
                        }

                        bool active = await dbContext.CustomerSessions
                            .AsNoTracking()
                            .AnyAsync(session =>
                                session.TokenIdHash == hash
                                && session.RevokedAtUtc == null
                                && session.ExpiresAtUtc > DateTime.UtcNow
                                && !session.Customer.IsDeleted
                                && session.Customer.Status == UserStatus.Active);
                        if (!active)
                        {
                            context.Fail("Customer session is no longer active.");
                        }
                    },
                    OnChallenge = static async context =>
                    {
                        context.HandleResponse();

                        if (!context.Response.HasStarted)
                        {
                            context.Response.Headers.WWWAuthenticate =
                                JwtBearerDefaults.AuthenticationScheme;
                            await ApiProblemDetailsWriter.WriteAsync(
                                context.HttpContext,
                                StatusCodes.Status401Unauthorized,
                                "Unauthorized",
                                "A valid bearer token is required to access this resource.");
                        }
                    },
                    OnForbidden = static async context =>
                    {
                        if (!context.Response.HasStarted)
                        {
                            await ApiProblemDetailsWriter.WriteAsync(
                                context.HttpContext,
                                StatusCodes.Status403Forbidden,
                                "Forbidden",
                                "You do not have permission to access this resource.");
                        }
                    },
                };
            });

        return services;
    }
}
