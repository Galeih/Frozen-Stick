using Microsoft.EntityFrameworkCore;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Infrastructure.Data.Repositories;

public class ContentRepository : IContentRepository
{
    private readonly AppDbContext _context;

    public ContentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ContentPost?> GetByIdAsync(Guid id)
    {
        return await _context.ContentPosts.FindAsync(id);
    }

    public async Task<List<ContentPost>> GetAllAsync(ContentType? typeFilter = null, ContentStatus? statusFilter = null)
    {
        var query = _context.ContentPosts.AsQueryable();

        if (typeFilter.HasValue)
        {
            query = query.Where(c => c.Type == typeFilter.Value);
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(c => c.Status == statusFilter.Value);
        }

        return await query.OrderByDescending(c => c.UpdatedAt).ToListAsync();
    }

    public async Task<List<ContentPost>> GetPublishedAsync(ContentType? typeFilter = null)
    {
        var query = _context.ContentPosts.Where(c => c.Status == ContentStatus.Published);

        if (typeFilter.HasValue)
        {
            query = query.Where(c => c.Type == typeFilter.Value);
        }

        return await query.OrderByDescending(c => c.PublishedAt).ToListAsync();
    }

    public async Task<ContentPost?> GetBySlugAsync(string slug)
    {
        return await _context.ContentPosts.FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task AddAsync(ContentPost content)
    {
        await _context.ContentPosts.AddAsync(content);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ContentPost content)
    {
        _context.ContentPosts.Update(content);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ContentPost content)
    {
        _context.ContentPosts.Remove(content);
        await _context.SaveChangesAsync();
    }
}
