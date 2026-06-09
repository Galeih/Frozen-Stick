using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Contents;
using Pierre.Web.Application.Services;

namespace Pierre.Web.Pages.Public;

public class IndexModel : PageModel
{
    private readonly ContentService _contentService;

    public IndexModel(ContentService contentService)
    {
        _contentService = contentService;
    }

    public List<ContentListItemDto> LatestContents { get; set; } = new();

    public async Task OnGetAsync()
    {
        var allPublished = await _contentService.GetPublishedAsync();
        LatestContents = allPublished.Take(3).ToList();
    }
}
