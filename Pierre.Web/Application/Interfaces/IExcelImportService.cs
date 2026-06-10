using Pierre.Web.Application.DTOs.Import;

namespace Pierre.Web.Application.Interfaces;

public interface IExcelImportService
{
    Task<ExcelAnalysisResult> AnalyzeAsync(string fileName, Stream stream);
    Task<ImportReportDto> ImportAsync(List<ImportRowDto> rows, string fileName);
}
