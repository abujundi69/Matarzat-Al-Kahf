using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Catalog.Categories;
public sealed partial class AdminCategoryService(MatarzatAlKahfDbContext dbContext, ICategoryRequestValidator validator, ISlugNormalizer slugNormalizer, IUploadedFileCleanupService fileCleanup, ILogger<AdminCategoryService> logger) : IAdminCategoryService
{
    private static readonly Action<ILogger, Guid, Exception?> LogCategoryCreated = LoggerMessage.Define<Guid>(LogLevel.Information, new EventId(2001, nameof(LogCategoryCreated)), "Category {CategoryId} was created");
    private static readonly Action<ILogger, Guid, Exception?> LogCategoryUpdated = LoggerMessage.Define<Guid>(LogLevel.Information, new EventId(2002, nameof(LogCategoryUpdated)), "Category {CategoryId} was updated");
    private static readonly Action<ILogger, Guid, Exception?> LogCategoryDeleted = LoggerMessage.Define<Guid>(LogLevel.Information, new EventId(2003, nameof(LogCategoryDeleted)), "Category {CategoryId} was soft deleted");
    private readonly MatarzatAlKahfDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ICategoryRequestValidator _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ISlugNormalizer _slugNormalizer = slugNormalizer ?? throw new ArgumentNullException(nameof(slugNormalizer));
    private readonly IUploadedFileCleanupService _fileCleanup = fileCleanup ?? throw new ArgumentNullException(nameof(fileCleanup));
    private readonly ILogger<AdminCategoryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
}
