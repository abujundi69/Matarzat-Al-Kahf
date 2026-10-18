using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Features.StoreSettings.DTOs;
using MatarzatAlKahf.Application.Features.StoreSettings.Interfaces;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class StoreSettingService(MatarzatAlKahfDbContext dbContext, IActiveLanguageResolver languageResolver, IAuditService auditService, IUploadedFileCleanupService fileCleanup) : IStoreSettingService
{
}
