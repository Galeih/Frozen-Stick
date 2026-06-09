using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Appointments;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Admin.Appointments;

public class DetailModel : PageModel
{
    private readonly AppointmentService _appointmentService;
    private readonly ILogger<DetailModel> _logger;

    public DetailModel(AppointmentService appointmentService, ILogger<DetailModel> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    public AppointmentDetailDto Appointment { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            Appointment = await _appointmentService.GetByIdAsync(id);
            return Page();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
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
            return NotFound();
        }
        catch (ValidationException ex)
        {
            TempData["Success"] = ex.Message;
        }

        return RedirectToPage(new { id });
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
            return NotFound();
        }
        catch (ValidationException ex)
        {
            TempData["Success"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }
}
