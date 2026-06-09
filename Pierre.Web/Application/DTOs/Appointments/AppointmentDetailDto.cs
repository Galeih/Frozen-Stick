namespace Pierre.Web.Application.DTOs.Appointments;

public class AppointmentDetailDto
{
    public Guid Id { get; set; }
    public Guid SlotId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string? RequesterEmail { get; set; }
    public string? RequesterPhone { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
