using System.ComponentModel.DataAnnotations;

namespace Pierre.Web.Application.DTOs.Invoices;

public class InvoiceListItemDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly IssuedAt { get; set; }
}

public class InvoiceDetailDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly IssuedAt { get; set; }
    public string? Notes { get; set; }
}

public class CreateInvoiceDto
{
    [Required(ErrorMessage = "Le client est obligatoire.")]
    public Guid ClientId { get; set; }

    [Required(ErrorMessage = "La référence est obligatoire.")]
    [StringLength(50, ErrorMessage = "La référence ne peut pas dépasser 50 caractères.")]
    public string Reference { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le montant est obligatoire.")]
    [Range(0.01, 999999.99, ErrorMessage = "Le montant doit être compris entre 0,01 et 999 999,99.")]
    public decimal Amount { get; set; }

    public string Status { get; set; } = "Pending";

    [Required(ErrorMessage = "La date est obligatoire.")]
    public DateOnly IssuedAt { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [StringLength(2000, ErrorMessage = "Les notes ne peuvent pas dépasser 2000 caractères.")]
    public string? Notes { get; set; }
}

public class UpdateInvoiceDto
{
    [Required(ErrorMessage = "La référence est obligatoire.")]
    [StringLength(50, ErrorMessage = "La référence ne peut pas dépasser 50 caractères.")]
    public string Reference { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le montant est obligatoire.")]
    [Range(0.01, 999999.99, ErrorMessage = "Le montant doit être compris entre 0,01 et 999 999,99.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Le statut est obligatoire.")]
    public string Status { get; set; } = "Pending";

    [Required(ErrorMessage = "La date est obligatoire.")]
    public DateOnly IssuedAt { get; set; }

    [StringLength(2000, ErrorMessage = "Les notes ne peuvent pas dépasser 2000 caractères.")]
    public string? Notes { get; set; }
}
