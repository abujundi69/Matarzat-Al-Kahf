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
    private static PaymentMethodResponse Map(StoreSetting setting, PaymentMethod method, LanguageCode language)
    {
        if (method == PaymentMethod.ElectronicPayment)
        {
            return new PaymentMethodResponse(method, false, DefaultInstructions(method, language));
        }

        StoreSettingTranslation? translation = setting.Translations.FirstOrDefault(item => item.Language == language) ?? setting.Translations.FirstOrDefault(item => item.Language == LanguageCode.Arabic);
        return new PaymentMethodResponse(method, ParseEnabled(setting.Value), translation?.Value ?? DefaultInstructions(method, language));
    }
}
