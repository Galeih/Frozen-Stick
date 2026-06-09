using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Clients;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Admin.Clients;

public class EditModel : PageModel
{
    private readonly ClientService _clientService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(ClientService clientService, ILogger<EditModel> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateClientDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var client = await _clientService.GetByIdAsync(Id);

            Input = new UpdateClientDto
            {
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Phone = client.Phone,
                BirthDate = client.BirthDate,
                Notes = client.Notes
            };

            return Page();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _clientService.UpdateAsync(Id, Input);
            TempData["Success"] = "Client modifié avec succès.";
            return RedirectToPage("Detail", new { id = Id });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
