using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.DataImports.DTOs;
using MatarzatAlKahf.Application.Features.DataImports.Interfaces;
using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.DataImports.Services;
public sealed partial class ImportService
{
    private static List<string[]> ReadRows(Stream stream)
    {
        try
        {
            using SpreadsheetDocument document = SpreadsheetDocument.Open(stream, false);
            WorkbookPart workbook = document.WorkbookPart ?? throw new InvalidDataException("Workbook is missing.");
            Sheet sheet = workbook.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault() ?? throw new InvalidDataException("Worksheet is missing.");
            WorksheetPart worksheet = (WorksheetPart)workbook.GetPartById(sheet.Id!);
            SharedStringTable? shared = workbook.SharedStringTablePart?.SharedStringTable;
            var rows = new List<string[]>();
            foreach (Row row in worksheet.Worksheet.Descendants<Row>().Take(MaximumRows + 2))
            {
                var values = new SortedDictionary<int, string>();
                foreach (Cell cell in row.Elements<Cell>())
                {
                    int column = GetColumnIndex(cell.CellReference?.Value);
                    values[column] = GetCellValue(cell, shared);
                }

                int maximum = values.Count == 0 ? -1 : values.Keys.Max();
                string[] cells = maximum < 0 ? [] : new string[maximum + 1];
                foreach ((int column, string value)in values)
                {
                    cells[column] = value;
                }

                rows.Add(cells);
            }

            if (rows.Count == 0 || rows.Count > MaximumRows + 1)
            {
                throw new ValidationException("Spreadsheet row count is invalid.", new Dictionary<string, string[]>());
            }

            return rows;
        }
        catch (OpenXmlPackageException exception)
        {
            throw new ValidationException("The uploaded file is not a valid Excel workbook.", new Dictionary<string, string[]> { ["file"] = [exception.Message] });
        }
    }
}
