using Microsoft.EntityFrameworkCore;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;

namespace Pierre.Web.Infrastructure.Data.Repositories;

public class AvailabilityRepository : IAvailabilityRepository
{
    private readonly AppDbContext _context;

    public AvailabilityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Availability>> GetAvailableSlotsAsync(DateTime from, DateTime to)
    {
        var fromDate = DateOnly.FromDateTime(from);
        var toDate = DateOnly.FromDateTime(to);
        var now = TimeOnly.FromDateTime(from);

        var slots = await _context.Availabilities
            .Include(a => a.Appointment)
            .Where(a => a.Date >= fromDate && a.Date <= toDate)
            .Where(a => !a.IsBlocked)
            .Where(a => a.Appointment == null)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        return slots
            .Where(a => a.Date != fromDate || a.StartTime > now)
            .ToList();
    }

    public async Task<Availability?> GetByIdAsync(Guid id)
    {
        return await _context.Availabilities
            .Include(a => a.Appointment)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Availability>> GetAllAsync(DateOnly from, DateOnly to)
    {
        return await _context.Availabilities
            .Include(a => a.Appointment)
            .Where(a => a.Date >= from && a.Date <= to)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<bool> HasOverlapAsync(DateOnly date, TimeOnly startTime, TimeOnly endTime, Guid? excludeId = null)
    {
        var query = _context.Availabilities
            .Where(a => a.Date == date)
            .Where(a => a.StartTime < endTime && a.EndTime > startTime);

        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(Availability availability)
    {
        await _context.Availabilities.AddAsync(availability);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Availability availability)
    {
        _context.Availabilities.Update(availability);
        await _context.SaveChangesAsync();
    }
}
