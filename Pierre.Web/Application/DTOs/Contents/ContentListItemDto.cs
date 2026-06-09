using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Application.DTOs.Contents;

public class ContentListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
