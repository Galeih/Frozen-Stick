using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Entities;

namespace Pierre.Web.Pages.Admin;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AuditService _auditService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        AuditService auditService,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _auditService = auditService;
        _logger = logger;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("Connexion réussie pour {Email}", Input.Email);
            _auditService.Log("Connexion réussie", Input.Email);
            return RedirectToPage("/Admin/Dashboard");
        }

        _auditService.Log("Échec de connexion", Input.Email);
        ErrorMessage = "Email ou mot de passe incorrect.";
        return Page();
    }
}

public class LoginInputModel
{
    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
