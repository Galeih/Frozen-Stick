using OfficeOpenXml;
using Pierre.Web.Application.DTOs.Import;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;
using Pierre.Web.Infrastructure.Data;

namespace Pierre.Web.Infrastructure.Import;

public class ExcelImportService : IExcelImportService
{
    private readonly IClientRepository _clientRepository;
    private readonly AppDbContext _context;
    private readonly AuditService _auditService;
    private readonly ILogger<ExcelImportService> _logger;

    private static readonly string[] ExpectedColumns = { "prénom", "nom", "email", "téléphone", "date de naissance" };

    public ExcelImportService(
        IClientRepository clientRepository,
        AppDbContext context,
        AuditService auditService,
        ILogger<ExcelImportService> logger)
    {
        _clientRepository = clientRepository;
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ExcelAnalysisResult> AnalyzeAsync(string fileName, Stream stream)
    {
        _auditService.Log("Import Excel démarré", fileName);

        var result = new ExcelAnalysisResult { FileName = fileName };

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        await package.LoadAsync(stream);

        var worksheet = package.Workbook.Worksheets[0];
        if (worksheet == null || worksheet.Dimension == null || worksheet.Dimension.Rows < 2)
        {
            return result;
        }

        var columnMap = BuildColumnMap(worksheet);

        if (!columnMap.ContainsKey("prénom") || !columnMap.ContainsKey("nom"))
        {
            return result;
        }

        var existingClients = await _clientRepository.GetAllIncludingArchivedAsync();
        var allImportEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var allImportPhones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int row = worksheet.Dimension.Start.Row + 1; row <= worksheet.Dimension.End.Row; row++)
        {
            var rowNumber = row;
            var firstName = GetCellValue(worksheet, row, columnMap["prénom"]);
            var lastName = GetCellValue(worksheet, row, columnMap["nom"]);
            var email = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("email"));
            var phone = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("téléphone"));
            var birthDateRaw = GetCellValue(worksheet, row, columnMap.GetValueOrDefault("date de naissance"));

            var errors = ValidateRow(firstName, lastName, email, phone);
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        RowNumber = rowNumber,
                        ColumnName = error.ColumnName,
                        Message = error.Message
                    });
                }
                continue;
            }

            var firstNameClean = firstName!.Trim();
            var lastNameClean = lastName!.Trim();
            var emailClean = email?.Trim();
            var phoneClean = phone?.Trim();
            var birthDate = TryParseDate(birthDateRaw);

            var duplicate = FindDuplicate(firstNameClean, lastNameClean, emailClean, phoneClean,
                existingClients, allImportEmails, allImportPhones);

            if (duplicate != null)
            {
                result.Duplicates.Add(new ImportDuplicateDto
                {
                    RowNumber = rowNumber,
                    FirstName = firstNameClean,
                    LastName = lastNameClean,
                    MatchedBy = duplicate
                });
                continue;
            }

            if (emailClean != null)
            {
                allImportEmails.Add(emailClean);
            }
            if (phoneClean != null)
            {
                allImportPhones.Add(phoneClean);
            }

            result.ValidRows.Add(new ImportRowDto
            {
                RowNumber = rowNumber,
                FirstName = firstNameClean,
                LastName = lastNameClean,
                Email = emailClean,
                Phone = phoneClean,
                BirthDate = birthDate?.ToString("yyyy-MM-dd")
            });
        }

        return result;
    }

    public async Task<ImportReportDto> ImportAsync(List<ImportRowDto> rows, string fileName)
    {
        var now = DateTime.UtcNow;
        var imported = 0;

        foreach (var row in rows)
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                FirstName = row.FirstName,
                LastName = row.LastName,
                Email = row.Email,
                Phone = row.Phone,
                BirthDate = TryParseDate(row.BirthDate),
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Clients.Add(client);
            imported++;
        }

        var importLog = new ImportedFile
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            ImportedAt = now,
            RowCount = rows.Count,
            ErrorCount = 0,
            Status = ImportStatus.Validated
        };

        _context.ImportedFiles.Add(importLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Excel import completed: {Count} clients imported from {FileName}", imported, fileName);
        _auditService.Log("Import Excel terminé", $"{fileName} - {imported} clients importés");

        return new ImportReportDto
        {
            FileName = fileName,
            TotalRows = rows.Count,
            ValidCount = rows.Count,
            ImportedCount = imported,
            ErrorCount = 0,
            DuplicateCount = 0,
            Status = "Validated",
            ImportedAt = now
        };
    }

    private static readonly Dictionary<string, HashSet<string>> ColumnAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["prénom"] = new(StringComparer.OrdinalIgnoreCase) { "prénom", "prenom", "first name", "firstname", "given name", "givenname" },
        ["nom"] = new(StringComparer.OrdinalIgnoreCase) { "nom", "last name", "lastname", "surname", "family name", "familyname" },
        ["email"] = new(StringComparer.OrdinalIgnoreCase) { "email", "e-mail", "mail", "adresse email", "email address" },
        ["téléphone"] = new(StringComparer.OrdinalIgnoreCase) { "téléphone", "telephone", "phone", "tel", "mobile", "portable", "phone number" },
        ["date de naissance"] = new(StringComparer.OrdinalIgnoreCase) { "date de naissance", "date de naiss", "birth date", "birthdate", "date of birth", "dob", "date naissance", "naissance" }
    };

    private static Dictionary<string, int> BuildColumnMap(ExcelWorksheet worksheet)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headerRow = worksheet.Dimension.Start.Row;

        for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
        {
            var header = worksheet.Cells[headerRow, col].Text?.Trim();
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            var headerNormalized = header.Normalize(System.Text.NormalizationForm.FormD);
            var headerClean = new string(headerNormalized
                .Where(c => char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray())
                .ToLowerInvariant()
                .Replace("-", "")
                .Replace("_", "")
                .Replace("  ", " ");

            foreach (var (key, aliases) in ColumnAliases)
            {
                if (aliases.Contains(header) || aliases.Contains(headerClean))
                {
                    map[key] = col;
                    break;
                }
            }
        }

        return map;
    }

    private static string? GetCellValue(ExcelWorksheet worksheet, int row, int? col)
    {
        if (col == null || col.Value <= 0)
        {
            return null;
        }

        var cell = worksheet.Cells[row, col.Value];
        var text = cell.Text?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return text;
    }

    private static List<(string ColumnName, string Message)> ValidateRow(
        string? firstName, string? lastName, string? email, string? phone)
    {
        var errors = new List<(string ColumnName, string Message)>();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            errors.Add(("Prénom", "Le prénom est obligatoire."));
        }
        else if (firstName.Trim().Length > 100)
        {
            errors.Add(("Prénom", "Le prénom ne peut pas dépasser 100 caractères."));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            errors.Add(("Nom", "Le nom est obligatoire."));
        }
        else if (lastName.Trim().Length > 100)
        {
            errors.Add(("Nom", "Le nom ne peut pas dépasser 100 caractères."));
        }

        if (!string.IsNullOrWhiteSpace(email) && email!.Trim().Length > 256)
        {
            errors.Add(("Email", "L'email ne peut pas dépasser 256 caractères."));
        }

        if (!string.IsNullOrWhiteSpace(phone) && phone!.Trim().Length > 20)
        {
            errors.Add(("Téléphone", "Le téléphone ne peut pas dépasser 20 caractères."));
        }

        return errors;
    }

    private static string? FindDuplicate(
        string firstName, string lastName,
        string? email, string? phone,
        List<Client> existingClients,
        HashSet<string> importEmails,
        HashSet<string> importPhones)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            if (importEmails.Contains(email))
            {
                return $"Email {email} déjà présent dans le fichier (ligne précédente)";
            }

            var existing = existingClients.FirstOrDefault(c =>
                c.Email != null && c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                return $"Email {email} déjà associé au client {existing.FirstName} {existing.LastName}";
            }
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            if (importPhones.Contains(phone))
            {
                return $"Téléphone {phone} déjà présent dans le fichier (ligne précédente)";
            }

            var existing = existingClients.FirstOrDefault(c =>
                c.Phone != null && c.Phone == phone);

            if (existing != null)
            {
                return $"Téléphone {phone} déjà associé au client {existing.FirstName} {existing.LastName}";
            }
        }

        return null;
    }

    private static DateOnly? TryParseDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (DateOnly.TryParse(raw, out var result))
        {
            return result;
        }

        if (DateTime.TryParse(raw, out var dt))
        {
            return DateOnly.FromDateTime(dt);
        }

        return null;
    }
}
