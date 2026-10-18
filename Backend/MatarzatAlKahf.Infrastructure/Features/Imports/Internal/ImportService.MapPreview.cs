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
    private static ImportPreviewResponse MapPreview(ImportOperation operation, IReadOnlyList<ImportRowError> errors) => new(operation.Id, operation.Type, operation.TotalRows, operation.ValidRows, operation.TotalRows - operation.ValidRows, errors, operation.ExpiresAtUtc);
}
