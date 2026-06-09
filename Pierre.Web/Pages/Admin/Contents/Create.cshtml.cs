using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pierre.Web.Application.DTOs.Contents;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Pages.Admin.Contents;

public class CreateModel : PageModel
{
    private readonly ContentService _contentService;

    public CreateModel(ContentService contentService)
    {
        _contentService = contentService;
    }

    [BindProperty]
    public CreateContentDto Input { get; set; } = new();

    public List<SelectListItem> TypeOptions { get; set; } = new();
    public List<SelectListItem> StatusOptions { get; set; } = new();

    public void OnGet()
    {
        LoadOptions();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadOptions();
            return Page();
        }

        try
        {
            await _contentService.CreateAsync(Input);
            TempData["Success"] = "Contenu créé avec succès.";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            LoadOptions();
            return Page();
        }
    }

    private void LoadOptions()
    {
        TypeOptions = Enum.GetValues<ContentType>()
            .Select(t => new SelectListItem(GetTypeDisplayName(t), t.ToString()))
            .ToList();

        StatusOptions = Enum.GetValues<ContentStatus>()
            .Select(s => new SelectListItem(GetStatusDisplayName(s), s.ToString()))
            .ToList();
    }

    private static string GetTypeDisplayName(ContentType type) => type switch
    {
        ContentType.Recipe => "Recette",
        ContentType.Article => "Article",
        ContentType.News => "Actualité",
        ContentType.Workshop => "Atelier",
        ContentType.Tip => "Conseil",
        _ => type.ToString()
    };

    private static string GetStatusDisplayName(ContentStatus status) => status switch
    {
        ContentStatus.Draft => "Brouillon",
        ContentStatus.Published => "Publié",
        _ => status.ToString()
    };
}
