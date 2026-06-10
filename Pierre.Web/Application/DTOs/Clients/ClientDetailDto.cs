namespace Pierre.Web.Application.DTOs.Clients;

public class ClientDetailDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Notes { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int AppointmentCount { get; set; }
    public List<ClientAppointmentDto> Appointments { get; set; } = new();
    public List<ClientNoteDto> ConsultationNotes { get; set; } = new();
    public List<ClientInvoiceDto> Invoices { get; set; } = new();
}

public class ClientInvoiceDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly IssuedAt { get; set; }
}

public class ClientAppointmentDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ClientNoteDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Recommendations { get; set; }
    public decimal? Weight { get; set; }
}
