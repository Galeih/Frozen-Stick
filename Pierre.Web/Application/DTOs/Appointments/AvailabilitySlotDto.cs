namespace Pierre.Web.Application.DTOs.Appointments;

public class AvailabilitySlotDto
{
    public Guid SlotId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
