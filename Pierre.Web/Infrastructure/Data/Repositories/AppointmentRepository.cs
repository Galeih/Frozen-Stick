using Microsoft.EntityFrameworkCore;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Infrastructure.Data.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments
            .Include(a => a.Slot)
            .Include(a => a.Client)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Appointment?> GetBySlotIdAsync(Guid slotId)
    {
        return await _context.Appointments
            .Include(a => a.Slot)
            .FirstOrDefaultAsync(a => a.SlotId == slotId);
    }

    public async Task<List<Appointment>> GetAllAsync(AppointmentStatus? statusFilter = null)
    {
        var query = _context.Appointments
            .Include(a => a.Slot)
            .Include(a => a.Client)
            .AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(a => a.Status == statusFilter.Value);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetPendingAsync()
    {
        return await _context.Appointments
            .Include(a => a.Slot)
            .Include(a => a.Client)
            .Where(a => a.Status == AppointmentStatus.Pending)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetUpcomingAcceptedAsync(int count)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _context.Appointments
            .Include(a => a.Slot)
            .Include(a => a.Client)
            .Where(a => a.Status == AppointmentStatus.Accepted)
            .Where(a => a.Slot.Date >= today)
            .OrderBy(a => a.Slot.Date)
            .ThenBy(a => a.Slot.StartTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
    }
}
