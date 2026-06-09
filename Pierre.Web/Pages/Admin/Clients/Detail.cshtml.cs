using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Clients;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Admin.Clients;

public class DetailModel : PageModel
{
    private readonly ClientService _clientService;

    public DetailModel(ClientService clientService)
    {
        _clientService = clientService;
    }

    public ClientDetailDto Client { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            Client = await _clientService.GetByIdAsync(id);
            return Page();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostArchiveAsync(Guid id)
    {
        try
        {
            await _clientService.ArchiveAsync(id);
            TempData["Success"] = "Client archivé avec succès.";
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return RedirectToPage(new { id });
    }
}
