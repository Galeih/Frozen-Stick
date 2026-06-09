using Pierre.Web.Domain.Entities;

namespace Pierre.Web.Application.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id);
    Task<List<Client>> GetAllActiveAsync();
    Task<List<Client>> GetAllIncludingArchivedAsync();
    Task<List<Client>> SearchAsync(string query);
    Task<Client?> FindByContactAsync(string? email, string? phone);
    Task AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task ArchiveAsync(Guid id);
}
