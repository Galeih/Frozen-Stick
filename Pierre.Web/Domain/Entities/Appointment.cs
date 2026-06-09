using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid? ClientId { get; set; }
    public Guid SlotId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string? RequesterEmail { get; set; }
    public string? RequesterPhone { get; set; }
    public string? Message { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Client? Client { get; set; }
    public Availability Slot { get; set; } = null!;
    public ConsultationNote? ConsultationNote { get; set; }
}
