using Microsoft.Extensions.Logging;
using Moq;
using Pierre.Web.Application.DTOs.ConsultationNotes;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Tests;

public class ConsultationNoteServiceTests
{
    private readonly Mock<IConsultationNoteRepository> _noteRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<ILogger<ConsultationNoteService>> _loggerMock;
    private readonly ConsultationNoteService _service;
    private static readonly AuditService AuditServiceStub = new(Mock.Of<ILogger<AuditService>>());

    public ConsultationNoteServiceTests()
    {
        _noteRepoMock = new Mock<IConsultationNoteRepository>();
        _clientRepoMock = new Mock<IClientRepository>();
        _loggerMock = new Mock<ILogger<ConsultationNoteService>>();

        _service = new ConsultationNoteService(
            _noteRepoMock.Object,
            _clientRepoMock.Object,
            AuditServiceStub,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithExistingClient_ShouldCreateNote()
    {
        var clientId = Guid.NewGuid();
        var dto = new CreateConsultationNoteDto
        {
            ClientId = clientId,
            Date = new DateTime(2026, 6, 10),
            Content = "Patient en bonne santé. IMC dans la norme. Conseils alimentaires prodigués.",
            Recommendations = "Maintenir une alimentation équilibrée.",
            Weight = 65.5m
        };

        var client = new Client
        {
            Id = clientId,
            FirstName = "Marie",
            LastName = "Martin"
        };

        _clientRepoMock
            .Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(client);

        ConsultationNote? savedNote = null;

        _noteRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ConsultationNote>()))
            .Callback<ConsultationNote>(n =>
            {
                savedNote = n;
                n.Id = Guid.NewGuid();
                n.Client = client;
                n.CreatedAt = DateTime.UtcNow;
                n.UpdatedAt = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);

        _noteRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => savedNote);

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(savedNote);
        Assert.Equal(clientId, savedNote.ClientId);
        Assert.Equal("Patient en bonne santé. IMC dans la norme. Conseils alimentaires prodigués.", savedNote.Content);
        Assert.Equal("Maintenir une alimentation équilibrée.", savedNote.Recommendations);
        Assert.Equal(65.5m, savedNote.Weight);
        Assert.Null(savedNote.AppointmentId);

        Assert.Equal(clientId, result.ClientId);
        Assert.Equal("Marie Martin", result.ClientName);

        _noteRepoMock.Verify(r => r.AddAsync(It.IsAny<ConsultationNote>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentClient_ShouldThrowNotFoundException()
    {
        var clientId = Guid.NewGuid();
        var dto = new CreateConsultationNoteDto
        {
            ClientId = clientId,
            Date = new DateTime(2026, 6, 10),
            Content = "Contenu de test"
        };

        _clientRepoMock
            .Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync((Client?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(dto));

        _noteRepoMock.Verify(r => r.AddAsync(It.IsAny<ConsultationNote>()), Times.Never);
    }

    [Fact]
    public async Task GetByClientIdAsync_ShouldReturnNotesInDescendingOrder()
    {
        var clientId = Guid.NewGuid();

        var notes = new List<ConsultationNote>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Date = new DateTime(2026, 6, 10),
                Content = "Première note - la plus récente",
                CreatedAt = new DateTime(2026, 6, 10, 10, 0, 0)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Date = new DateTime(2026, 5, 20),
                Content = "Deuxième note",
                CreatedAt = new DateTime(2026, 5, 20, 14, 0, 0)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Date = new DateTime(2026, 4, 15),
                Content = "Troisième note - la plus ancienne",
                CreatedAt = new DateTime(2026, 4, 15, 9, 0, 0)
            }
        };

        _noteRepoMock
            .Setup(r => r.GetByClientIdAsync(clientId))
            .ReturnsAsync(notes);

        var result = await _service.GetByClientIdAsync(clientId);

        Assert.Equal(3, result.Count);
        Assert.True(result[0].Date >= result[1].Date);
        Assert.True(result[1].Date >= result[2].Date);
    }
}
