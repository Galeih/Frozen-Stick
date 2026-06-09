using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<Appointment?> GetBySlotIdAsync(Guid slotId);
    Task<List<Appointment>> GetAllAsync(AppointmentStatus? statusFilter = null);
    Task<List<Appointment>> GetPendingAsync();
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
}
