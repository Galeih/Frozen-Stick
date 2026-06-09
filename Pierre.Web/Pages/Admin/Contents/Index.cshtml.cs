using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pierre.Web.Application.DTOs.Contents;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Pages.Admin.Contents;

public class IndexModel : PageModel
{
    private readonly ContentService _contentService;

    public IndexModel(ContentService contentService)
    {
        _contentService = contentService;
    }

    public List<ContentListItemDto> Contents { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? TypeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public List<SelectListItem> TypeOptions { get; set; } = new();
    public List<SelectListItem> StatusOptions { get; set; } = new();

    public async Task OnGetAsync()
    {
        LoadFilterOptions();

        ContentType? type = ParseTypeFilter(TypeFilter);
        ContentStatus? status = ParseStatusFilter(StatusFilter);

        Contents = await _contentService.GetAllForAdminAsync(type, status);
    }

    public async Task<IActionResult> OnPostPublishAsync(Guid id)
    {
        await _contentService.PublishAsync(id);
        TempData["Success"] = "Contenu publié avec succès.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnpublishAsync(Guid id)
    {
        await _contentService.UnpublishAsync(id);
        TempData["Success"] = "Contenu dépublié avec succès.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _contentService.DeleteAsync(id);
        TempData["Success"] = "Contenu supprimé avec succès.";
        return RedirectToPage();
    }

    private void LoadFilterOptions()
    {
        TypeOptions = Enum.GetValues<ContentType>()
            .Select(t => new SelectListItem(GetTypeDisplayName(t), t.ToString()))
            .ToList();

        StatusOptions = Enum.GetValues<ContentStatus>()
            .Select(s => new SelectListItem(GetStatusDisplayName(s), s.ToString()))
            .ToList();
    }

    private static ContentType? ParseTypeFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<ContentType>(value, out var type) ? type : null;
    }

    private static ContentStatus? ParseStatusFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<ContentStatus>(value, out var status) ? status : null;
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
