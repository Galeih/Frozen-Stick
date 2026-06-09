namespace Pierre.Web.Domain.Entities;

public class NotificationLog
{
    public Guid Id { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}
