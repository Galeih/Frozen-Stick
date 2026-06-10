namespace Pierre.Web.Application.DTOs.ConsultationNotes;

public class ConsultationNoteListItemDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string ContentPreview { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public DateTime CreatedAt { get; set; }
}
