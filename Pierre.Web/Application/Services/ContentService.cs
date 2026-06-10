using Pierre.Web.Application.DTOs.Contents;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;
using Pierre.Web.Domain.Exceptions;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Pierre.Web.Application.Services;

public class ContentService
{
    private readonly IContentRepository _repository;
    private readonly AuditService _auditService;
    private readonly ILogger<ContentService> _logger;

    public ContentService(IContentRepository repository, AuditService auditService, ILogger<ContentService> logger)
    {
        _repository = repository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<List<ContentListItemDto>> GetAllForAdminAsync(
        ContentType? typeFilter = null, ContentStatus? statusFilter = null)
    {
        var contents = await _repository.GetAllAsync(typeFilter, statusFilter);

        return contents.Select(MapToListItem).ToList();
    }

    public async Task<List<ContentListItemDto>> GetPublishedAsync(ContentType? typeFilter = null)
    {
        var contents = await _repository.GetPublishedAsync(typeFilter);

        return contents.Select(MapToListItem).ToList();
    }

    public async Task<ContentDetailDto> GetBySlugAsync(string slug)
    {
        var content = await _repository.GetBySlugAsync(slug);

        if (content == null || content.Status != ContentStatus.Published)
        {
            throw new NotFoundException(nameof(ContentPost), slug);
        }

        return MapToDetail(content);
    }

    public async Task<ContentDetailDto> GetByIdAsync(Guid id)
    {
        var content = await _repository.GetByIdAsync(id);

        if (content == null)
        {
            throw new NotFoundException(nameof(ContentPost), id);
        }

        return MapToDetail(content);
    }

    public async Task<ContentDetailDto> CreateAsync(CreateContentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Length < 3)
        {
            throw new ValidationException("Le titre doit contenir au moins 3 caractères.");
        }

        var now = DateTime.UtcNow;
        var content = new ContentPost
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Slug = GenerateSlug(dto.Title),
            Type = dto.Type,
            Status = dto.Status,
            Body = dto.Body,
            ImagePath = dto.ImagePath,
            PublishedAt = dto.Status == ContentStatus.Published ? now : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddAsync(content);

        _logger.LogInformation("Content created: {Title} (Slug: {Slug})", content.Title, content.Slug);

        return MapToDetail(content);
    }

    public async Task<ContentDetailDto> UpdateAsync(Guid id, UpdateContentDto dto)
    {
        var content = await _repository.GetByIdAsync(id);

        if (content == null)
        {
            throw new NotFoundException(nameof(ContentPost), id);
        }

        var now = DateTime.UtcNow;

        content.Title = dto.Title.Trim();
        content.Slug = GenerateSlug(dto.Title);
        content.Type = dto.Type;
        content.Body = dto.Body;
        content.ImagePath = dto.ImagePath;
        content.UpdatedAt = now;

        if (content.Status != dto.Status)
        {
            content.Status = dto.Status;
            content.PublishedAt = dto.Status == ContentStatus.Published ? now : null;
        }

        await _repository.UpdateAsync(content);

        _logger.LogInformation("Content updated: {Title} (Status: {Status})", content.Title, content.Status);

        return MapToDetail(content);
    }

    public async Task PublishAsync(Guid id)
    {
        var content = await _repository.GetByIdAsync(id);

        if (content == null)
        {
            throw new NotFoundException(nameof(ContentPost), id);
        }

        content.Status = ContentStatus.Published;
        content.PublishedAt ??= DateTime.UtcNow;
        content.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(content);

        _logger.LogInformation("Content published: {Title}", content.Title);
        _auditService.Log("Contenu publié", $"{content.Title} (ID: {content.Id})");
    }

    public async Task UnpublishAsync(Guid id)
    {
        var content = await _repository.GetByIdAsync(id);

        if (content == null)
        {
            throw new NotFoundException(nameof(ContentPost), id);
        }

        content.Status = ContentStatus.Draft;
        content.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(content);

        _logger.LogInformation("Content unpublished: {Title}", content.Title);
        _auditService.Log("Contenu dépublié", $"{content.Title} (ID: {content.Id})");
    }

    public async Task DeleteAsync(Guid id)
    {
        var content = await _repository.GetByIdAsync(id);

        if (content == null)
        {
            throw new NotFoundException(nameof(ContentPost), id);
        }

        await _repository.DeleteAsync(content);

        _logger.LogInformation("Content deleted: {Title}", content.Title);
    }

    private static ContentListItemDto MapToListItem(ContentPost content)
    {
        return new ContentListItemDto
        {
            Id = content.Id,
            Title = content.Title,
            Slug = content.Slug,
            Type = content.Type.ToString(),
            Status = content.Status.ToString(),
            UpdatedAt = content.UpdatedAt,
            PublishedAt = content.PublishedAt
        };
    }

    private static ContentDetailDto MapToDetail(ContentPost content)
    {
        return new ContentDetailDto
        {
            Id = content.Id,
            Title = content.Title,
            Slug = content.Slug,
            Type = content.Type.ToString(),
            Status = content.Status.ToString(),
            Body = content.Body,
            ImagePath = content.ImagePath,
            PublishedAt = content.PublishedAt,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt
        };
    }

    private static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        var normalized = title.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return slug;
    }
}
