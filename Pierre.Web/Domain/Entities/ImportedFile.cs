using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Domain.Entities;

public class ImportedFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
    public int RowCount { get; set; }
    public int ErrorCount { get; set; }
    public ImportStatus Status { get; set; }
}
