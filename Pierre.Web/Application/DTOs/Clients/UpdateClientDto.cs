using System.ComponentModel.DataAnnotations;

namespace Pierre.Web.Application.DTOs.Clients;

public class UpdateClientDto
{
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

    public DateOnly? BirthDate { get; set; }

    [StringLength(2000, ErrorMessage = "Les notes ne peuvent pas dépasser 2000 caractères.")]
    public string? Notes { get; set; }
}
