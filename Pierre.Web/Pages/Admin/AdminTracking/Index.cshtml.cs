using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Invoices;
using Pierre.Web.Application.Services;

namespace Pierre.Web.Pages.Admin.AdminTracking;

public class IndexModel : PageModel
{
    private readonly InvoiceService _invoiceService;

    public List<InvoiceListItemDto> Invoices { get; set; } = new();
    public string? CurrentStatus { get; set; }

    public IndexModel(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public async Task OnGetAsync(string? status)
    {
        CurrentStatus = status;

        Domain.Enums.InvoiceStatus? statusFilter = status switch
        {
            "Pending" => Domain.Enums.InvoiceStatus.Pending,
            "Paid" => Domain.Enums.InvoiceStatus.Paid,
            "Cancelled" => Domain.Enums.InvoiceStatus.Cancelled,
            _ => null
        };

        Invoices = await _invoiceService.GetAllAsync(statusFilter);
    }
}
