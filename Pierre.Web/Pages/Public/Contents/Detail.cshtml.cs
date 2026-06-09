using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Contents;
using Pierre.Web.Application.Services;

namespace Pierre.Web.Pages.Public.Contents;

public class DetailModel : PageModel
{
    private readonly ContentService _contentService;

    public DetailModel(ContentService contentService)
    {
        _contentService = contentService;
    }

    public ContentDetailDto? Article { get; set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        try
        {
            Article = await _contentService.GetBySlugAsync(slug);
            return Page();
        }
        catch (Exception)
        {
            return NotFound();
        }
    }
}
