using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pierre.Web.Application.DTOs.ConsultationNotes;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Admin.ConsultationNotes;

public class CreateModel : PageModel
{
    private readonly ConsultationNoteService _noteService;
    private readonly IClientRepository _clientRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public CreateModel(
        ConsultationNoteService noteService,
        IClientRepository clientRepository,
        IAppointmentRepository appointmentRepository)
    {
        _noteService = noteService;
        _clientRepository = clientRepository;
        _appointmentRepository = appointmentRepository;
    }

    [BindProperty]
    public CreateConsultationNoteDto Input { get; set; } = new();

    public string ClientName { get; set; } = string.Empty;
    public List<SelectListItem> AppointmentOptions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid clientId)
    {
        Input.ClientId = clientId;

        var client = await _clientRepository.GetByIdAsync(clientId);

        if (client == null)
        {
            return NotFound();
        }

        ClientName = $"{client.FirstName} {client.LastName}";
        await LoadAppointmentOptions(clientId);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var client = await _clientRepository.GetByIdAsync(Input.ClientId);

            if (client != null)
            {
                ClientName = $"{client.FirstName} {client.LastName}";
                await LoadAppointmentOptions(Input.ClientId);
            }

            return Page();
        }

        try
        {
            var note = await _noteService.CreateAsync(Input);
            TempData["Success"] = "Note de consultation créée avec succès.";
            return RedirectToPage("/Admin/Clients/Detail", new { id = Input.ClientId });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private async Task LoadAppointmentOptions(Guid clientId)
    {
        var appointments = await _appointmentRepository.GetAllAsync(Domain.Enums.AppointmentStatus.Accepted);

        AppointmentOptions = appointments
            .Where(a => a.ClientId == clientId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Slot.Date:dd/MM/yyyy} à {a.Slot.StartTime:HH\\hmm}"
            })
            .ToList();
    }
}
