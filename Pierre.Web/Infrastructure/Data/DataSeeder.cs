using Microsoft.EntityFrameworkCore;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Infrastructure.Data;

public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(AppDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDevelopmentDataAsync()
    {
        if (await _context.Clients.AnyAsync())
        {
            _logger.LogInformation("Development data already exists, skipping.");
            return;
        }

        var clients = SeedClients();
        _context.Clients.AddRange(clients);

        var posts = SeedContentPosts();
        _context.ContentPosts.AddRange(posts);

        var availabilities = SeedAvailabilities();
        _context.Availabilities.AddRange(availabilities);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Development data seeded: {ClientCount} clients, {PostCount} contents, {SlotCount} slots",
            clients.Count, posts.Count, availabilities.Count);
    }

    private static List<Client> SeedClients()
    {
        var now = DateTime.UtcNow;

        return new List<Client>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Sophie",
                LastName = "Martin",
                Email = "sophie.martin@email.fr",
                Phone = "0612345678",
                BirthDate = new DateOnly(1985, 3, 15),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Lucas",
                LastName = "Bernard",
                Email = "lucas.bernard@email.fr",
                Phone = "0623456789",
                BirthDate = new DateOnly(1992, 7, 22),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Camille",
                LastName = "Dubois",
                Email = "camille.dubois@email.fr",
                BirthDate = new DateOnly(1978, 11, 8),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Thomas",
                LastName = "Petit",
                Phone = "0634567890",
                BirthDate = new DateOnly(1995, 5, 30),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Marie",
                LastName = "Leroy",
                Email = "marie.leroy@email.fr",
                Phone = "0645678901",
                CreatedAt = now,
                UpdatedAt = now
            }
        };
    }

    private static List<ContentPost> SeedContentPosts()
    {
        var now = DateTime.UtcNow;

        return new List<ContentPost>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Salade de quinoa aux légumes d'été",
                Slug = "salade-quinoa-legumes-ete",
                Type = ContentType.Recipe,
                Status = ContentStatus.Published,
                Body = "<p>Une recette fraîche et équilibrée pour les beaux jours.</p><h2>Ingrédients</h2><ul><li>200g de quinoa</li><li>Tomates cerises</li><li>Concombre</li><li>Poivron</li><li>Huile d'olive</li></ul><h2>Préparation</h2><p>Cuire le quinoa, laisser refroidir, ajouter les légumes coupés et assaisonner.</p>",
                PublishedAt = now.AddDays(-5),
                CreatedAt = now.AddDays(-7),
                UpdatedAt = now.AddDays(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Les bienfaits d'une alimentation anti-inflammatoire",
                Slug = "bienfaits-alimentation-anti-inflammatoire",
                Type = ContentType.Article,
                Status = ContentStatus.Published,
                Body = "<p>L'alimentation joue un rôle clé dans la gestion de l'inflammation chronique. Découvrez les aliments à privilégier et ceux à éviter.</p><h2>Aliments anti-inflammatoires</h2><ul><li>Les poissons gras (saumon, sardine)</li><li>Les fruits rouges</li><li>Les légumes verts</li><li>Les huiles végétales de qualité</li></ul>",
                PublishedAt = now.AddDays(-3),
                CreatedAt = now.AddDays(-10),
                UpdatedAt = now.AddDays(-3)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Atelier cuisine : équilibrer ses assiettes au quotidien",
                Slug = "atelier-equilibrer-assiettes-quotidien",
                Type = ContentType.Workshop,
                Status = ContentStatus.Published,
                Body = "<p>Un atelier pratique pour apprendre à composer des repas équilibrés sans se prendre la tête.</p><p>Au programme :</p><ul><li>Les bases de la nutrition</li><li>La méthode de l'assiette équilibrée</li><li>Des recettes simples et rapides</li></ul><p>Durée : 2h. Matériel fourni.</p>",
                PublishedAt = now.AddDays(-1),
                CreatedAt = now.AddDays(-15),
                UpdatedAt = now.AddDays(-1)
            }
        };
    }

    private static List<Availability> SeedAvailabilities()
    {
        var slots = new List<Availability>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var timeSlots = new[] { (9, 10), (10, 11), (11, 12), (14, 15), (15, 16) };

        for (var dayOffset = 1; dayOffset <= 14; dayOffset++)
        {
            var date = today.AddDays(dayOffset);

            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            foreach (var (start, end) in timeSlots)
            {
                slots.Add(new Availability
                {
                    Id = Guid.NewGuid(),
                    Date = date,
                    StartTime = new TimeOnly(start, 0),
                    EndTime = new TimeOnly(end, 0),
                    IsBlocked = false
                });
            }
        }

        return slots;
    }
}
