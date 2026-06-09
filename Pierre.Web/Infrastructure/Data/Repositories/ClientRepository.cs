using Microsoft.EntityFrameworkCore;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;

namespace Pierre.Web.Infrastructure.Data.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        return await _context.Clients
            .Include(c => c.Appointments)
                .ThenInclude(a => a.Slot)
            .Include(c => c.ConsultationNotes)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Client>> GetAllActiveAsync()
    {
        return await _context.Clients
            .Where(c => !c.IsArchived)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();
    }

    public async Task<List<Client>> GetAllIncludingArchivedAsync()
    {
        return await _context.Clients
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();
    }

    public async Task<List<Client>> SearchAsync(string query)
    {
        var search = query.Trim().ToLower();

        return await _context.Clients
            .Where(c => !c.IsArchived)
            .Where(c => c.FirstName.ToLower().Contains(search)
                || c.LastName.ToLower().Contains(search)
                || (c.FirstName + " " + c.LastName).ToLower().Contains(search))
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();
    }

    public async Task<Client?> FindByContactAsync(string? email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var query = _context.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(c => c.Email == email);
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            query = query.Where(c => c.Phone == phone);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task AddAsync(Client client)
    {
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Client client)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid id)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client != null)
        {
            client.IsArchived = true;
            client.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
