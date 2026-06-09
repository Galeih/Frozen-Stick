using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pierre.Web.Application.DTOs.Contents;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Pages.Admin.Contents;

public class EditModel : PageModel
{
    private readonly ContentService _contentService;

    public EditModel(ContentService contentService)
    {
        _contentService = contentService;
    }

    [BindProperty]
    public UpdateContentDto Input { get; set; } = new();

    public List<SelectListItem> TypeOptions { get; set; } = new();
    public List<SelectListItem> StatusOptions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            var detail = await _contentService.GetByIdAsync(id);
            Input = new UpdateContentDto
            {
                Title = detail.Title,
                Type = Enum.Parse<ContentType>(detail.Type),
                Status = Enum.Parse<ContentStatus>(detail.Status),
                Body = detail.Body,
                ImagePath = detail.ImagePath
            };

            LoadOptions();
            return Page();
        }
        catch (Exception)
        {
            TempData["Error"] = "Contenu introuvable.";
            return RedirectToPage("Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid)
        {
            LoadOptions();
            return Page();
        }

        try
        {
            await _contentService.UpdateAsync(id, Input);
            TempData["Success"] = "Contenu modifié avec succès.";
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
