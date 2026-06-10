using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Invoices;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Admin.AdminTracking;

public class EditModel : PageModel
{
    private readonly InvoiceService _invoiceService;

    public EditModel(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateInvoiceDto Input { get; set; } = new();

    public string ClientName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var invoice = await _invoiceService.GetByIdAsync(Id);

            ClientName = invoice.ClientName;

            Input = new UpdateInvoiceDto
            {
                Reference = invoice.Reference,
                Amount = invoice.Amount,
                Status = invoice.Status,
                IssuedAt = invoice.IssuedAt,
                Notes = invoice.Notes
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
            try
            {
                var invoice = await _invoiceService.GetByIdAsync(Id);
                ClientName = invoice.ClientName;
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            return Page();
        }

        try
        {
            await _invoiceService.UpdateAsync(Id, Input);
            TempData["Success"] = "Entrée modifiée avec succès.";
            return RedirectToPage("Index");
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
