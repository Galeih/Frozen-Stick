namespace Pierre.Web.Application.DTOs.ConsultationNotes;

public class ConsultationNoteDetailDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public string? AppointmentInfo { get; set; }
    public DateTime Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Recommendations { get; set; }
    public decimal? Weight { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
