using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<List<Invoice>> GetAllAsync(InvoiceStatus? statusFilter = null);
    Task<List<Invoice>> GetByClientIdAsync(Guid clientId);
    Task AddAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);
}
