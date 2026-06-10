using Microsoft.EntityFrameworkCore;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;

namespace Pierre.Web.Infrastructure.Data.Repositories;

public class ConsultationNoteRepository : IConsultationNoteRepository
{
    private readonly AppDbContext _context;

    public ConsultationNoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConsultationNote?> GetByIdAsync(Guid id)
    {
        return await _context.ConsultationNotes
            .Include(n => n.Client)
            .Include(n => n.Appointment)
                .ThenInclude(a => a!.Slot)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<List<ConsultationNote>> GetByClientIdAsync(Guid clientId)
    {
        return await _context.ConsultationNotes
            .Where(n => n.ClientId == clientId)
            .OrderByDescending(n => n.Date)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(ConsultationNote note)
    {
        await _context.ConsultationNotes.AddAsync(note);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ConsultationNote note)
    {
        _context.ConsultationNotes.Update(note);
        await _context.SaveChangesAsync();
    }
}
