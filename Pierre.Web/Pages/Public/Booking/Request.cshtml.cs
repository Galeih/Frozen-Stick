using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pierre.Web.Application.DTOs.Appointments;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Public.Booking;

public class RequestModel : PageModel
{
    private readonly AppointmentService _appointmentService;
    private readonly ILogger<RequestModel> _logger;

    public RequestModel(AppointmentService appointmentService, ILogger<RequestModel> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    public List<AvailabilitySlotDto> AvailableSlots { get; set; } = new();
    public SelectList SlotSelectList { get; set; } = null!;

    [BindProperty]
    public AppointmentRequestDto Input { get; set; } = new();

    [TempData]
    public bool RequestSuccess { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        AvailableSlots = await _appointmentService.GetAvailableSlotsAsync();
        SlotSelectList = new SelectList(
            AvailableSlots.Select(s => new
            {
                Value = s.SlotId.ToString(),
                Text = $"{s.Date:dddd dd MMMM yyyy} - {s.StartTime:HH\\hmm}"
            }),
            "Value",
            "Text");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        try
        {
            await _appointmentService.RequestAppointmentAsync(Input);
            RequestSuccess = true;
            return RedirectToPage();
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await OnGetAsync();
            return Page();
        }
        catch (ConflictException ex)
        {
            ErrorMessage = ex.Message;
            return RedirectToPage();
        }
    }
}
