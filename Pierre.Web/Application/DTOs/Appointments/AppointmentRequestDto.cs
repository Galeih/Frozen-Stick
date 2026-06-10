using System.ComponentModel.DataAnnotations;

namespace Pierre.Web.Application.DTOs.Appointments;

public class AppointmentRequestDto
{
    [Required(ErrorMessage = "Le créneau est obligatoire.")]
    public Guid SlotId { get; set; }

    [Required(ErrorMessage = "Le prénom est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide.")]
    [StringLength(256, ErrorMessage = "L'email ne peut pas dépasser 256 caractères.")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide.")]
    [StringLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères.")]
    public string? Phone { get; set; }

    [StringLength(1000, ErrorMessage = "Le message ne peut pas dépasser 1000 caractères.")]
    public string? Message { get; set; }
}
