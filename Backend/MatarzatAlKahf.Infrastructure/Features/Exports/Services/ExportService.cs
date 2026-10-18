using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Application.Features.Exports.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Exports.Services;
public sealed partial class ExportService(MatarzatAlKahfDbContext dbContext) : IExportService
{
    private const int MaximumRows = 50000;
    private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
