using System.ComponentModel.DataAnnotations;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Application.DTOs.Contents;

public class CreateContentDto
{
    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Le titre doit contenir entre 3 et 120 caractères.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le type est obligatoire.")]
    public ContentType Type { get; set; }

    [Required(ErrorMessage = "Le statut est obligatoire.")]
    public ContentStatus Status { get; set; }

    [Required(ErrorMessage = "Le contenu est obligatoire.")]
    public string Body { get; set; } = string.Empty;

    public string? ImagePath { get; set; }
}
