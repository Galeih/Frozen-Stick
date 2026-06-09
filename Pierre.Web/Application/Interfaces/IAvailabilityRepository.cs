using Pierre.Web.Domain.Entities;

namespace Pierre.Web.Application.Interfaces;

public interface IAvailabilityRepository
{
    Task<List<Availability>> GetAvailableSlotsAsync(DateTime from, DateTime to);
    Task<Availability?> GetByIdAsync(Guid id);
    Task<List<Availability>> GetAllAsync(DateOnly from, DateOnly to);
    Task<bool> HasOverlapAsync(DateOnly date, TimeOnly startTime, TimeOnly endTime, Guid? excludeId = null);
    Task AddAsync(Availability availability);
    Task UpdateAsync(Availability availability);
}
