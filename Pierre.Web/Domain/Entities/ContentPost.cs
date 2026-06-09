using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Domain.Entities;

public class ContentPost
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ContentType Type { get; set; }
    public ContentStatus Status { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
