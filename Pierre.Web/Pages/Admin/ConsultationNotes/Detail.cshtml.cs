using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs.ConsultationNotes;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Pages.Admin.ConsultationNotes;

public class DetailModel : PageModel
{
    private readonly ConsultationNoteService _noteService;

    public DetailModel(ConsultationNoteService noteService)
    {
        _noteService = noteService;
    }

    public ConsultationNoteDetailDto Note { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            Note = await _noteService.GetByIdAsync(id);
            return Page();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
