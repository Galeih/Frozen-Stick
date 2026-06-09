namespace Pierre.Web.Domain.Entities;

public class Client
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

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ConsultationNote> ConsultationNotes { get; set; } = new List<ConsultationNote>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
