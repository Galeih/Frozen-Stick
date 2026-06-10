namespace Pierre.Web.Application.Services;

public class AuditService
{
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    public void Log(string action, string details)
    {
        _logger.LogInformation(
            "[AUDIT] {Action} | {Details} | {Timestamp:yyyy-MM-dd HH:mm:ss}",
            action, details, DateTime.UtcNow);
    }
}
