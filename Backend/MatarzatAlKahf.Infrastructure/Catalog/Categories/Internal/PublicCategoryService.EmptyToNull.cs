using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Catalog.Categories;
public sealed partial class PublicCategoryService
{
    private static string? EmptyToNull(string value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
