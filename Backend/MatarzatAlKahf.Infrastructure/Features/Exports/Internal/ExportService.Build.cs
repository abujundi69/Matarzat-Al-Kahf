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
    private static ExportFile Build(string sheetName, IReadOnlyList<string> headers, IEnumerable<string[]> rows, string fileName)
    {
        var stream = new MemoryStream();
        using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        {
            WorkbookPart workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            using (OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());
                WriteRow(writer, headers);
                foreach (string[] row in rows)
                {
                    WriteRow(writer, row);
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = sheetName });
            workbookPart.Workbook.Save();
        }

        stream.Position = 0;
        return new ExportFile(stream, ContentType, fileName);
    }
}
