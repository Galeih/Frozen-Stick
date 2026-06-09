namespace Pierre.Web.Domain.Entities;

public class ConsultationNote
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public DateTime Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Recommendations { get; set; }
    public decimal? Weight { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Client Client { get; set; } = null!;
    public Appointment? Appointment { get; set; }
}
