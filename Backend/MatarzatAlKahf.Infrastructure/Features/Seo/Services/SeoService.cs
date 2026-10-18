using System.Text;
using System.Xml;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Features.Seo.DTOs;
using MatarzatAlKahf.Application.Features.Seo.Interfaces;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Seo.Services;
public sealed partial class SeoService(MatarzatAlKahfDbContext dbContext, IAuditService auditService, IUploadedFileCleanupService fileCleanup) : ISeoService
{
}
