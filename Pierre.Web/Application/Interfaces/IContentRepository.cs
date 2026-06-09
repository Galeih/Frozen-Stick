using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Application.Interfaces;

public interface IContentRepository
{
    Task<ContentPost?> GetByIdAsync(Guid id);
    Task<List<ContentPost>> GetAllAsync(ContentType? typeFilter = null, ContentStatus? statusFilter = null);
    Task<List<ContentPost>> GetPublishedAsync(ContentType? typeFilter = null);
    Task<ContentPost?> GetBySlugAsync(string slug);
    Task AddAsync(ContentPost content);
    Task UpdateAsync(ContentPost content);
    Task DeleteAsync(ContentPost content);
}
