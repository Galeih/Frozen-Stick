namespace Pierre.Web.Application.DTOs.Appointments;

public class PlanningSlotDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsBlocked { get; set; }
    public bool HasAppointment { get; set; }
    public string? AppointmentStatus { get; set; }
}
