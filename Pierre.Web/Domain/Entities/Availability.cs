namespace Pierre.Web.Domain.Entities;

public class Availability
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsBlocked { get; set; }

    public Appointment? Appointment { get; set; }
}
