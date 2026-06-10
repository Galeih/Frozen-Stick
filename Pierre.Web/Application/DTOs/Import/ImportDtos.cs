namespace Pierre.Web.Application.DTOs.Import;

public class ImportRowDto
{
    public int RowNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? BirthDate { get; set; }
}

public class ImportErrorDto
{
    public int RowNumber { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ImportDuplicateDto
{
    public int RowNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MatchedBy { get; set; } = string.Empty;
}

public class ExcelAnalysisResult
{
    public string FileName { get; set; } = string.Empty;
    public List<ImportRowDto> ValidRows { get; set; } = new();
    public List<ImportErrorDto> Errors { get; set; } = new();
    public List<ImportDuplicateDto> Duplicates { get; set; } = new();

    public int TotalRows => ValidRows.Count + Errors.Count + Duplicates.Count;
    public int ValidCount => ValidRows.Count;
    public int ErrorCount => Errors.Count;
    public int DuplicateCount => Duplicates.Count;
}

public class ImportReportDto
{
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ValidCount { get; set; }
    public int ErrorCount { get; set; }
    public int DuplicateCount { get; set; }
    public int ImportedCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
}
