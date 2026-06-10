using Moq;
using Microsoft.Extensions.Logging;
using Pierre.Web.Application.DTOs.Contents;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Tests;

public class ContentServiceTests
{
    private readonly Mock<IContentRepository> _repositoryMock;
    private readonly Mock<ILogger<ContentService>> _loggerMock;
    private readonly ContentService _service;
    private static readonly AuditService AuditServiceStub = new(Mock.Of<ILogger<AuditService>>());

    public ContentServiceTests()
    {
        _repositoryMock = new Mock<IContentRepository>();
        _loggerMock = new Mock<ILogger<ContentService>>();
        _service = new ContentService(_repositoryMock.Object, AuditServiceStub, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidTitle_ShouldCreateContent()
    {
        var dto = new CreateContentDto
        {
            Title = "Salade de quinoa aux légumes",
            Type = ContentType.Recipe,
            Status = ContentStatus.Published,
            Body = "<p>Recette détaillée</p>"
        };

        ContentPost? savedContent = null;

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ContentPost>()))
            .Callback<ContentPost>(c => savedContent = c)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(savedContent);
        Assert.Equal("Salade de quinoa aux légumes", savedContent.Title);
        Assert.Equal("salade-de-quinoa-aux-legumes", savedContent.Slug);
        Assert.Equal(ContentType.Recipe, savedContent.Type);
        Assert.Equal(ContentStatus.Published, savedContent.Status);
        Assert.NotNull(savedContent.PublishedAt);
        Assert.Equal(savedContent.Title, result.Title);
        Assert.Equal(savedContent.Slug, result.Slug);
    }

    [Fact]
    public async Task CreateAsync_WithShortTitle_ShouldThrowValidationException()
    {
        var dto = new CreateContentDto
        {
            Title = "AB",
            Type = ContentType.Article,
            Status = ContentStatus.Draft,
            Body = "<p>Contenu</p>"
        };

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ContentPost>()), Times.Never);
    }

    [Fact]
    public async Task PublishAsync_WithExistingContent_ShouldPublish()
    {
        var contentId = Guid.NewGuid();
        var content = new ContentPost
        {
            Id = contentId,
            Title = "Article test",
            Slug = "article-test",
            Type = ContentType.Article,
            Status = ContentStatus.Draft,
            Body = "<p>Corps</p>",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(contentId))
            .ReturnsAsync(content);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<ContentPost>()))
            .Returns(Task.CompletedTask);

        await _service.PublishAsync(contentId);

        Assert.Equal(ContentStatus.Published, content.Status);
        Assert.NotNull(content.PublishedAt);
        _repositoryMock.Verify(r => r.UpdateAsync(content), Times.Once);
    }

    [Fact]
    public async Task GetBySlugAsync_WithNonExistentSlug_ShouldThrowNotFoundException()
    {
        var slug = "slug-inexistant";

        _repositoryMock
            .Setup(r => r.GetBySlugAsync(slug))
            .ReturnsAsync((ContentPost?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetBySlugAsync(slug));
    }
}
