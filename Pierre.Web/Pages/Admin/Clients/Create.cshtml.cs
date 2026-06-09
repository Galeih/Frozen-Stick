using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Clients;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Admin.Clients;

public class CreateModel : PageModel
{
    private readonly ClientService _clientService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ClientService clientService, ILogger<CreateModel> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    [BindProperty]
    public CreateClientDto Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = await _clientService.CreateAsync(Input);

        TempData["Success"] = $"Client « {client.FirstName} {client.LastName} » créé avec succès.";
        return RedirectToPage("Detail", new { id = client.Id });
    }
}
