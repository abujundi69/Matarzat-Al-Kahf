using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Payments.Services;
public sealed partial class PaymentMethodService
{
    public async Task<PaymentMethodResponse> UpdateAsync(PaymentMethod method, UpdatePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        if (method == PaymentMethod.ElectronicPayment && request.IsEnabled)
        {
            throw new ConflictException("Electronic payment cannot be enabled until a verified payment gateway is configured.");
        }

        if (string.IsNullOrWhiteSpace(request.ArabicInstructions) || string.IsNullOrWhiteSpace(request.EnglishInstructions) || request.ArabicInstructions.Length > 2000 || request.EnglishInstructions.Length > 2000)
        {
            throw ValidationException.For(nameof(request), "Arabic and English instructions are required and limited to 2000 characters.");
        }

        string key = Key(method);
        StoreSetting? setting = await dbContext.StoreSettings
            .Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.Key == key && !item.IsDeleted, cancellationToken);
        if (setting is null)
        {
            setting = new StoreSetting
            {
                Key = key,
                Group = Group,
                IsPublic = true,
                IsLocalized = true,
                Description = $"Configuration for {method}."
            };
            dbContext.StoreSettings.Add(setting);
        }

        setting.Value = request.IsEnabled.ToString();
        UpsertTranslation(setting, LanguageCode.Arabic, request.ArabicInstructions.Trim());
        UpsertTranslation(setting, LanguageCode.English, request.EnglishInstructions.Trim());
        setting.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.RecordAsync("payment-method.updated", nameof(StoreSetting), setting.Id, $"{method} enabled={request.IsEnabled}", cancellationToken);
        return Map(setting, method, languageResolver.GetCurrentLanguage());
    }
}
