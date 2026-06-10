namespace Pierre.Web.Application.DTOs;

public class DashboardDto
{
    public List<DashboardAppointmentDto> UpcomingAppointments { get; set; } = new();
    public int PendingRequestsCount { get; set; }
    public List<DashboardAppointmentDto> RecentPendingRequests { get; set; } = new();
    public List<DashboardClientDto> RecentClients { get; set; } = new();
    public List<DashboardContentDto> RecentContents { get; set; } = new();
}

public class DashboardAppointmentDto
{
    public Guid Id { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DashboardClientDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime CreatedAt { get; set; }
}

public class DashboardContentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
}
