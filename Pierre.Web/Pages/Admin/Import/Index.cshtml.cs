using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Import;
using Pierre.Web.Application.Interfaces;

namespace Pierre.Web.Pages.Admin.Import;

public class IndexModel : PageModel
{
    private readonly IExcelImportService _importService;

    public IndexModel(IExcelImportService importService)
    {
        _importService = importService;
    }

    [BindProperty]
    public IFormFile? Upload { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Upload == null || Upload.Length == 0)
        {
            ModelState.AddModelError("Upload", "Veuillez sélectionner un fichier.");
            return Page();
        }

        if (!Upload.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Upload", "Seuls les fichiers .xlsx sont acceptés.");
            return Page();
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await using var stream = new MemoryStream();
        await Upload.CopyToAsync(stream);
        stream.Position = 0;

        var result = await _importService.AnalyzeAsync(Upload.FileName, stream);
        
        if (result.TotalRows == 0)
        {
            TempData["Error"] = "Le fichier ne contient aucune donnée ou les colonnes attendues sont introuvables.";
            return Page();
        }

        TempData["ValidRows"] = JsonSerializer.Serialize(result.ValidRows, jsonOptions);
        TempData["Errors"] = JsonSerializer.Serialize(result.Errors, jsonOptions);
        TempData["Duplicates"] = JsonSerializer.Serialize(result.Duplicates, jsonOptions);
        TempData["FileName"] = Upload.FileName;

        return RedirectToPage("Preview");
    }
}
