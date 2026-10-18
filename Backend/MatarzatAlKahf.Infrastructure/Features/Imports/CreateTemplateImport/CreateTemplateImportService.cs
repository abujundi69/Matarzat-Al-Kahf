using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.DataImports.DTOs;
using MatarzatAlKahf.Application.Features.DataImports.Interfaces;
using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.DataImports.Services;
public sealed partial class ImportService
{
    public ExportFile CreateTemplate(ImportType type, LanguageCode language)
    {
        string[] headers = type == ImportType.Products ? language == LanguageCode.Arabic ? ["SKU", "معرف التصنيف", "الاسم العربي", "الاسم الإنجليزي", "السعر", "الكمية", "المقاس", "اللون", "حد إعادة الطلب"] : ["SKU", "CategoryId", "ArabicName", "EnglishName", "Price", "Quantity", "Size", "Color", "ReorderLevel"] : language == LanguageCode.Arabic ? ["SKU", "الكمية", "حد إعادة الطلب"] : ["SKU", "Quantity", "ReorderLevel"];
        return BuildTemplate(headers, type == ImportType.Products ? "products-import-template.xlsx" : "inventory-import-template.xlsx");
    }
}
