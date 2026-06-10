using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pierre.Web.Application.DTOs.Invoices;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Admin.AdminTracking;

public class CreateModel : PageModel
{
    private readonly InvoiceService _invoiceService;
    private readonly ClientService _clientService;

    public CreateModel(InvoiceService invoiceService, ClientService clientService)
    {
        _invoiceService = invoiceService;
        _clientService = clientService;
    }

    [BindProperty]
    public CreateInvoiceDto Input { get; set; } = new();

    public List<SelectListItem> ClientOptions { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadClientsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadClientsAsync();
            return Page();
        }

        try
        {
            var invoice = await _invoiceService.CreateAsync(Input);
            TempData["Success"] = $"Entrée « {invoice.Reference} » créée avec succès.";
            return RedirectToPage("Index");
        }
        catch (NotFoundException)
        {
            ModelState.AddModelError("Input.ClientId", "Le client sélectionné n'existe pas.");
            await LoadClientsAsync();
            return Page();
        }
    }

    private async Task LoadClientsAsync()
    {
        var clients = await _clientService.GetAllAsync();

        ClientOptions = clients.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.FirstName} {c.LastName}"
        }).ToList();
    }
}
