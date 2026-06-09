namespace Pierre.Web.Application.DTOs.Appointments;

public class AppointmentListItemDto
{
    public Guid Id { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public DateTime CreatedAt { get; set; }
}
