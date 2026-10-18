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
    private static string GetPaymentInstructions(PaymentMethod method, LanguageCode language) => (method, language) switch
    {
        (PaymentMethod.CashOnDelivery, LanguageCode.Arabic) => "يتم تحصيل المبلغ عند التسليم.",
        (PaymentMethod.BankTransfer, LanguageCode.Arabic) => "حوّل المبلغ ثم ارفع إثبات التحويل من تفاصيل الطلب.",
        (PaymentMethod.ElectronicWallet, LanguageCode.Arabic) => "حوّل المبلغ إلى المحفظة ثم ارفع الإثبات من تفاصيل الطلب.",
        (PaymentMethod.CreditCard, LanguageCode.Arabic) => "تابع إلى مزود الدفع الإلكتروني لإكمال العملية.",
        (PaymentMethod.CashOnDelivery, _) => "Payment is collected on delivery.",
        (PaymentMethod.BankTransfer, _) => "Transfer the amount, then upload proof from the order details.",
        (PaymentMethod.ElectronicWallet, _) => "Transfer to the wallet, then upload proof from the order details.",
        _ => "Continue through the configured online payment provider."
    };
}
