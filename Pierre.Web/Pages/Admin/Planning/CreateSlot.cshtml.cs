using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;

namespace Pierre.Web.Pages.Admin.Planning;

public class CreateSlotModel : PageModel
{
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly ILogger<CreateSlotModel> _logger;

    public CreateSlotModel(IAvailabilityRepository availabilityRepository, ILogger<CreateSlotModel> logger)
    {
        _availabilityRepository = availabilityRepository;
        _logger = logger;
    }

    [BindProperty]
    public CreateSlotInput Input { get; set; } = new();

    public void OnGet()
    {
        if (Request.Query.TryGetValue("date", out var dateStr)
            && DateOnly.TryParse(dateStr, out var date))
        {
            Input.Date = date;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.StartTime >= Input.EndTime)
        {
            ModelState.AddModelError(string.Empty, "L'heure de fin doit être postérieure à l'heure de début.");
            return Page();
        }

        var hasOverlap = await _availabilityRepository.HasOverlapAsync(
            Input.Date, Input.StartTime, Input.EndTime);

        if (hasOverlap)
        {
            ModelState.AddModelError(string.Empty,
                "Un créneau existe déjà sur cette plage horaire pour cette date.");
            return Page();
        }

        var slot = new Availability
        {
            Id = Guid.NewGuid(),
            Date = Input.Date,
            StartTime = Input.StartTime,
            EndTime = Input.EndTime,
            IsBlocked = Input.IsBlocked
        };

        await _availabilityRepository.AddAsync(slot);

        _logger.LogInformation(
            "Availability slot created: {Date} {StartTime}-{EndTime} (Blocked: {IsBlocked})",
            slot.Date, slot.StartTime, slot.EndTime, slot.IsBlocked);

        TempData["Success"] = "Créneau créé avec succès.";
        return RedirectToPage("Index");
    }

    public class CreateSlotInput
    {
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public TimeOnly StartTime { get; set; } = new(9, 0);
        public TimeOnly EndTime { get; set; } = new(10, 0);
        public bool IsBlocked { get; set; }
    }
}
