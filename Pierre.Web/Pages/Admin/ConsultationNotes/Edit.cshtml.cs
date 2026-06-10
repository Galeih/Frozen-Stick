using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.ConsultationNotes;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Admin.ConsultationNotes;

public class EditModel : PageModel
{
    private readonly ConsultationNoteService _noteService;

    public EditModel(ConsultationNoteService noteService)
    {
        _noteService = noteService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateConsultationNoteDto Input { get; set; } = new();

    public string ClientName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var note = await _noteService.GetByIdAsync(Id);

            ClientName = note.ClientName;
            Input = new UpdateConsultationNoteDto
            {
                Date = note.Date,
                Content = note.Content,
                Recommendations = note.Recommendations,
                Weight = note.Weight
            };

            return Page();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            try
            {
                var note = await _noteService.GetByIdAsync(Id);
                ClientName = note.ClientName;
            }
            catch
            {
                return NotFound();
            }

            return Page();
        }

        try
        {
            var note = await _noteService.UpdateAsync(Id, Input);
            TempData["Success"] = "Note modifiée avec succès.";
            return RedirectToPage("Detail", new { id = Id });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
