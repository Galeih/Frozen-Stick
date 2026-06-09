using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Contents;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Pages.Public.Contents;

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

    public async Task OnGetAsync()
    {
        ContentType? type = null;

        if (!string.IsNullOrWhiteSpace(TypeFilter)
            && Enum.TryParse<ContentType>(TypeFilter, out var parsed))
        {
            type = parsed;
        }

        Contents = await _contentService.GetPublishedAsync(type);
    }
}
