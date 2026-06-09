using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Clients;
using Pierre.Web.Application.Services;

namespace Pierre.Web.Pages.Admin.Clients;

public class IndexModel : PageModel
{
    private readonly ClientService _clientService;

    public IndexModel(ClientService clientService)
    {
        _clientService = clientService;
    }

    public List<ClientListItemDto> Clients { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool IncludeArchived { get; set; }

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            Clients = await _clientService.SearchAsync(SearchQuery);
        }
        else
        {
            Clients = await _clientService.GetAllAsync(IncludeArchived);
        }
    }
}
