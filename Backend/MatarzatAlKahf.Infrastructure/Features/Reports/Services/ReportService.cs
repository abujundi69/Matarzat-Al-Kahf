using System.Globalization;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Reports.DTOs;
using MatarzatAlKahf.Application.Features.Reports.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Reports.Services;
public sealed partial class ReportService(
    MatarzatAlKahfDbContext dbContext,
    IActiveLanguageResolver languageResolver) : IReportService
{
}
