using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.Appointments;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;

namespace Pierre.Web.Pages.Admin.Planning;

public class IndexModel : PageModel
{
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IAvailabilityRepository availabilityRepository, ILogger<IndexModel> logger)
    {
        _availabilityRepository = availabilityRepository;
        _logger = logger;
    }

    public List<PlanningSlotDto> Slots { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int WeekOffset { get; set; } = 0;

    public DateOnly WeekStart { get; set; }

    public async Task OnGetAsync()
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var daysToMonday = ((int)today.DayOfWeek - 1 + 7) % 7;
        WeekStart = today.AddDays(-daysToMonday + WeekOffset * 7);
        var weekEnd = WeekStart.AddDays(6);

        var slots = await _availabilityRepository.GetAllAsync(WeekStart, weekEnd);

        Slots = slots.Select(s => new PlanningSlotDto
        {
            Id = s.Id,
            Date = s.Date,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsBlocked = s.IsBlocked,
            HasAppointment = s.Appointment != null,
            AppointmentStatus = s.Appointment?.Status.ToString()
        }).ToList();
    }
}
