using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pierre.Web.Configuration;
using Pierre.Web.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Pierre.Web.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOptions<AdminSeedSettings> _adminSettings;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IOptions<AdminSeedSettings> adminSettings,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _adminSettings = adminSettings;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Applying pending migrations...");
        await _context.Database.MigrateAsync();

        if (!await _userManager.Users.AnyAsync())
        {
            var settings = _adminSettings.Value;

            var admin = new ApplicationUser
            {
                UserName = settings.Email,
                Email = settings.Email,
                FirstName = settings.FirstName,
                LastName = settings.LastName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(admin, settings.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Admin account created: {Email}", admin.Email);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create admin account: {Errors}", errors);
            }
        }
        else
        {
            _logger.LogInformation("Admin account already exists, skipping seed.");
        }
    }
}
