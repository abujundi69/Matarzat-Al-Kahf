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
public sealed partial class ExportService
{
    private static void WriteRow(OpenXmlWriter writer, IEnumerable<string> values)
    {
        writer.WriteStartElement(new Row());
        foreach (string value in values)
        {
            writer.WriteElement(new Cell { DataType = CellValues.InlineString, InlineString = new InlineString(new Text(value ?? string.Empty)) });
        }

        writer.WriteEndElement();
    }
}
