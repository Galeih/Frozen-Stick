using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OfficeOpenXml;
using Pierre.Web.Application.DTOs.Import;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Infrastructure.Data;
using Pierre.Web.Infrastructure.Import;

namespace Pierre.Tests;

public class ExcelImportServiceTests
{
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<ILogger<ExcelImportService>> _loggerMock;
    private readonly ExcelImportService _service;
    private static readonly AuditService AuditServiceStub = new(Mock.Of<ILogger<AuditService>>());

    public ExcelImportServiceTests()
    {
        _clientRepositoryMock = new Mock<IClientRepository>();
        _loggerMock = new Mock<ILogger<ExcelImportService>>();

        _clientRepositoryMock
            .Setup(r => r.GetAllIncludingArchivedAsync())
            .ReturnsAsync(new List<Client>());

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        _service = new ExcelImportService(_clientRepositoryMock.Object, context, AuditServiceStub, _loggerMock.Object);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenRowHasNoLastName_ShouldReturnError()
    {
        var stream = CreateExcelStream(
            new[] { "Prénom", "Nom" },
            new[] { "Marie", " " });

        var result = await _service.AnalyzeAsync("test.xlsx", stream);

        Assert.Empty(result.ValidRows);
        Assert.Single(result.Errors);
        Assert.Contains(result.Errors, e => e.ColumnName == "Nom" && e.Message.Contains("obligatoire"));
        Assert.Empty(result.Duplicates);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenDuplicateEmail_ShouldDetectDuplicate()
    {
        var existing = new List<Client>
        {
            new Client
            {
                Id = Guid.NewGuid(),
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean@test.fr"
            }
        };

        _clientRepositoryMock
            .Setup(r => r.GetAllIncludingArchivedAsync())
            .ReturnsAsync(existing);

        var stream = CreateExcelStream(
            new[] { "Prénom", "Nom", "Email" },
            new[] { "Pierre", "Martin", "jean@test.fr" });

        var result = await _service.AnalyzeAsync("test.xlsx", stream);

        Assert.Empty(result.ValidRows);
        Assert.Empty(result.Errors);
        Assert.Single(result.Duplicates);
        Assert.Contains(result.Duplicates, d => d.MatchedBy.Contains("jean@test.fr"));
    }

    [Fact]
    public async Task ImportAsync_ShouldSaveOnlyValidRows()
    {
        var rows = new List<ImportRowDto>
        {
            new ImportRowDto
            {
                RowNumber = 2,
                FirstName = "Alice",
                LastName = "Durand",
                Email = "alice@test.fr",
                Phone = "0102030405"
            },
            new ImportRowDto
            {
                RowNumber = 3,
                FirstName = "Bob",
                LastName = "Martin",
                Email = "bob@test.fr"
            }
        };

        var report = await _service.ImportAsync(rows, "test.xlsx");

        Assert.Equal(2, report.ImportedCount);
        Assert.Equal(2, report.ValidCount);
        Assert.Equal("test.xlsx", report.FileName);
        Assert.Equal("Validated", report.Status);
    }

    private static MemoryStream CreateExcelStream(string[] headers, params string[][] rows)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sheet1");

        for (int c = 0; c < headers.Length; c++)
        {
            worksheet.Cells[1, c + 1].Value = headers[c];
        }

        for (int r = 0; r < rows.Length; r++)
        {
            for (int c = 0; c < headers.Length; c++)
            {
                var val = c < rows[r].Length ? rows[r][c] : null;
                if (val != null)
                {
                    worksheet.Cells[r + 2, c + 1].Value = val;
                }
            }
        }

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        return stream;
    }
}
