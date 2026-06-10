using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Import;

namespace Pierre.Web.Pages.Admin.Import;

public class ResultModel : PageModel
{
    public ImportReportDto Report { get; set; } = new();

    public IActionResult OnGet()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var reportJson = TempData.Peek("Report") as string;

        if (string.IsNullOrWhiteSpace(reportJson))
        {
            return RedirectToPage("Index");
        }

        Report = JsonSerializer.Deserialize<ImportReportDto>(reportJson, jsonOptions) ?? new();

        return Page();
    }
}
