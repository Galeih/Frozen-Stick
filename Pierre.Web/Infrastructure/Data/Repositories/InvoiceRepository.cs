using Microsoft.EntityFrameworkCore;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;
using Pierre.Web.Infrastructure.Data;

namespace Pierre.Web.Infrastructure.Data.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        return await _context.Invoices
            .Include(i => i.Client)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Invoice>> GetAllAsync(InvoiceStatus? statusFilter = null)
    {
        var query = _context.Invoices
            .Include(i => i.Client)
            .AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(i => i.Status == statusFilter.Value);
        }

        return await query
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetByClientIdAsync(Guid clientId)
    {
        return await _context.Invoices
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
    }
}
