using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Infrastructure.Data;

namespace Pierre.Web.Pages.Public;

public class ContactModel : PageModel
{
    private readonly AppDbContext _context;

    public ContactModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ContactInputModel Input { get; set; } = new();

    public bool MessageSent { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = new ContactRequest
        {
            Id = Guid.NewGuid(),
            Name = Input.Name.Trim(),
            Email = Input.Email?.Trim(),
            Message = Input.Message?.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.ContactRequests.Add(request);
        await _context.SaveChangesAsync();

        MessageSent = true;
        ModelState.Clear();
        Input = new ContactInputModel();

        return Page();
    }
}

public class ContactInputModel
{
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "L'email n'est pas valide.")]
    [MaxLength(256)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Le message est obligatoire.")]
    public string? Message { get; set; }
}
