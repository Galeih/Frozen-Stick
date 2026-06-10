using System.ComponentModel.DataAnnotations;

namespace Pierre.Web.Application.DTOs.ConsultationNotes;

public class UpdateConsultationNoteDto
{
    [Required(ErrorMessage = "La date est obligatoire.")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Le contenu est obligatoire.")]
    [StringLength(10000, ErrorMessage = "Le contenu ne peut pas dépasser 10 000 caractères.")]
    public string Content { get; set; } = string.Empty;

    [StringLength(5000, ErrorMessage = "Les recommandations ne peuvent pas dépasser 5 000 caractères.")]
    public string? Recommendations { get; set; }

    [Range(0, 300, ErrorMessage = "Le poids doit être compris entre 0 et 300 kg.")]
    public decimal? Weight { get; set; }
}
