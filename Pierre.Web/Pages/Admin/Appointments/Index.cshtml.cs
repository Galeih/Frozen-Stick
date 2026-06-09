using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Appointments;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Enums;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Admin.Appointments;

public class IndexModel : PageModel
{
    private readonly AppointmentService _appointmentService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(AppointmentService appointmentService, ILogger<IndexModel> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    public List<AppointmentListItemDto> Appointments { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public async Task OnGetAsync()
    {
        AppointmentStatus? status = null;

        if (!string.IsNullOrWhiteSpace(StatusFilter)
            && Enum.TryParse<AppointmentStatus>(StatusFilter, out var parsed))
        {
            status = parsed;
        }

        Appointments = await _appointmentService.GetAllForAdminAsync(status);
    }

    public async Task<IActionResult> OnPostAcceptAsync(Guid id)
    {
        try
        {
            await _appointmentService.AcceptAsync(id);
            TempData["Success"] = "Rendez-vous accepté avec succès.";
        }
        catch (NotFoundException)
        {
            TempData["Success"] = "Rendez-vous introuvable.";
        }
        catch (ValidationException ex)
        {
            TempData["Success"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRefuseAsync(Guid id)
    {
        try
        {
            await _appointmentService.RefuseAsync(id);
            TempData["Success"] = "Rendez-vous refusé.";
        }
        catch (NotFoundException)
        {
            TempData["Success"] = "Rendez-vous introuvable.";
        }
        catch (ValidationException ex)
        {
            TempData["Success"] = ex.Message;
        }

        return RedirectToPage();
    }
}
