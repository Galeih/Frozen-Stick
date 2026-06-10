using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pierre.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public int HttpStatusCode { get; set; }
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    private readonly ILogger<ErrorModel> _logger;

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public void OnGet(int? statusCode)
    {
        HttpStatusCode = statusCode ?? 0;
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        if (HttpStatusCode > 0)
        {
            _logger.LogWarning("Page error {StatusCode} for {Path}", HttpStatusCode, Request.Path);
        }
    }
}
