using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Import;
using Pierre.Web.Application.Interfaces;

namespace Pierre.Web.Pages.Admin.Import;

public class PreviewModel : PageModel
{
    private readonly IExcelImportService _importService;

    public PreviewModel(IExcelImportService importService)
    {
        _importService = importService;
    }

    public string FileName { get; set; } = string.Empty;
    public List<ImportRowDto> ValidRows { get; set; } = new();
    public List<ImportErrorDto> Errors { get; set; } = new();
    public List<ImportDuplicateDto> Duplicates { get; set; } = new();

    public IActionResult OnGet()
    {
        if (!LoadFromTempData())
        {
            return RedirectToPage("Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!LoadFromTempData())
        {
            return RedirectToPage("Index");
        }

        if (ValidRows.Count == 0)
        {
            return RedirectToPage("Index");
        }

        var report = await _importService.ImportAsync(ValidRows, FileName);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        TempData["Report"] = JsonSerializer.Serialize(report, jsonOptions);

        return RedirectToPage("Result");
    }

    private bool LoadFromTempData()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var validRowsJson = TempData.Peek("ValidRows") as string;
        var errorsJson = TempData.Peek("Errors") as string;
        var duplicatesJson = TempData.Peek("Duplicates") as string;
        var fileName = TempData.Peek("FileName") as string;

        if (string.IsNullOrWhiteSpace(validRowsJson) || string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        FileName = fileName;

        if (!string.IsNullOrWhiteSpace(validRowsJson))
        {
            ValidRows = JsonSerializer.Deserialize<List<ImportRowDto>>(validRowsJson, jsonOptions) ?? new();
        }

        if (!string.IsNullOrWhiteSpace(errorsJson))
        {
            Errors = JsonSerializer.Deserialize<List<ImportErrorDto>>(errorsJson, jsonOptions) ?? new();
        }

        if (!string.IsNullOrWhiteSpace(duplicatesJson))
        {
            Duplicates = JsonSerializer.Deserialize<List<ImportDuplicateDto>>(duplicatesJson, jsonOptions) ?? new();
        }

        return true;
    }
}
