using MatarzatAlKahf.Application.Features.DataImports.DTOs;
using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.DataImports.Interfaces;
public interface ICreateTemplateImportService
{
    ExportFile CreateTemplate(ImportType type, LanguageCode language);
}
