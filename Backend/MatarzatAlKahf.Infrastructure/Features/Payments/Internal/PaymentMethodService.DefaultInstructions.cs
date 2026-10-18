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
    private static string DefaultInstructions(PaymentMethod method, LanguageCode language) => (method, language) switch
    {
        (PaymentMethod.ElectronicPayment, LanguageCode.Arabic) => "الدفع الإلكتروني — غير متاح حالياً",
        (PaymentMethod.ElectronicPayment, _) => "Electronic Payment — Currently unavailable",
        (PaymentMethod.CashOnDelivery, LanguageCode.Arabic) => "يتم الدفع عند الاستلام.",
        (PaymentMethod.BankTransfer, LanguageCode.Arabic) => "حوّل المبلغ ثم ارفع إثبات التحويل.",
        (PaymentMethod.ElectronicWallet, LanguageCode.Arabic) => "حوّل إلى المحفظة ثم ارفع الإثبات.",
        (PaymentMethod.CreditCard, LanguageCode.Arabic) => "الدفع الإلكتروني غير مهيأ.",
        (PaymentMethod.CashOnDelivery, _) => "Payment is collected on delivery.",
        (PaymentMethod.BankTransfer, _) => "Transfer the amount, then upload proof.",
        (PaymentMethod.ElectronicWallet, _) => "Transfer to the wallet, then upload proof.",
        _ => "Online payment is not configured."
    };
}
