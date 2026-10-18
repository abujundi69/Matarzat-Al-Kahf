using System.Globalization;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Reports.DTOs;
using MatarzatAlKahf.Application.Features.Reports.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Reports.Services;
public sealed partial class ReportService
{
    private static void Validate(ReportQuery query)
    {
        PaginationRules.Validate(query.Page, query.PageSize);
        if (query.FromUtc.Kind != DateTimeKind.Utc || query.ToUtc.Kind != DateTimeKind.Utc || query.FromUtc >= query.ToUtc || query.ToUtc - query.FromUtc > TimeSpan.FromDays(731))
        {
            throw ValidationException.For(nameof(query), "A valid UTC date range of at most two years is required.");
        }
    }
}
