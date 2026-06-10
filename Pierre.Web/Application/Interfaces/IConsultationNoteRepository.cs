using Pierre.Web.Domain.Entities;

namespace Pierre.Web.Application.Interfaces;

public interface IConsultationNoteRepository
{
    Task<ConsultationNote?> GetByIdAsync(Guid id);
    Task<List<ConsultationNote>> GetByClientIdAsync(Guid clientId);
    Task AddAsync(ConsultationNote note);
    Task UpdateAsync(ConsultationNote note);
}
