using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Domain.Entities;

public class Invoice
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateOnly IssuedAt { get; set; }
    public string? Notes { get; set; }

    public Client Client { get; set; } = null!;
}
