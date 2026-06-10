using Microsoft.Extensions.Logging;
using Moq;
using Pierre.Web.Application.DTOs.Clients;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Application.Services;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Tests;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _repositoryMock;
    private readonly Mock<ILogger<ClientService>> _loggerMock;
    private readonly ClientService _service;
    private static readonly AuditService AuditServiceStub = new(Mock.Of<ILogger<AuditService>>());

    public ClientServiceTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
        _loggerMock = new Mock<ILogger<ClientService>>();
        _service = new ClientService(_repositoryMock.Object, AuditServiceStub, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateClient()
    {
        var dto = new CreateClientDto
        {
            FirstName = "Marie",
            LastName = "Martin",
            Email = "marie.martin@exemple.fr",
            Phone = "0612345678",
            Notes = "Cliente suivie pour rééquilibrage alimentaire"
        };

        Client? savedClient = null;

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Client>()))
            .Callback<Client>(c =>
            {
                savedClient = c;
                c.Id = Guid.NewGuid();
                c.CreatedAt = DateTime.UtcNow;
                c.UpdatedAt = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => savedClient);

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(savedClient);
        Assert.Equal("Marie", savedClient.FirstName);
        Assert.Equal("Martin", savedClient.LastName);
        Assert.Equal("marie.martin@exemple.fr", savedClient.Email);
        Assert.Equal("0612345678", savedClient.Phone);
        Assert.False(savedClient.IsArchived);

        Assert.Equal("Marie", result.FirstName);
        Assert.Equal("Martin", result.LastName);
        Assert.Equal("marie.martin@exemple.fr", result.Email);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Client>()), Times.Once);
    }

    [Fact]
    public async Task ArchiveAsync_ShouldSetIsArchivedToTrue()
    {
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            FirstName = "Marie",
            LastName = "Martin",
            Email = "marie@exemple.fr",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(client);

        _repositoryMock
            .Setup(r => r.ArchiveAsync(clientId))
            .Callback(() => client.IsArchived = true)
            .Returns(Task.CompletedTask);

        await _service.ArchiveAsync(clientId);

        Assert.True(client.IsArchived);
        _repositoryMock.Verify(r => r.ArchiveAsync(clientId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithIncludeArchivedFalse_ShouldNotReturnArchivedClients()
    {
        var activeClients = new List<Client>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Jean", LastName = "Dupont", IsArchived = false },
            new() { Id = Guid.NewGuid(), FirstName = "Marie", LastName = "Martin", IsArchived = false }
        };

        _repositoryMock
            .Setup(r => r.GetAllActiveAsync())
            .ReturnsAsync(activeClients);

        var result = await _service.GetAllAsync(includeArchived: false);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.False(c.IsArchived));
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingClients()
    {
        var query = "martin";

        var matchingClients = new List<Client>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Marie", LastName = "Martin", IsArchived = false }
        };

        _repositoryMock
            .Setup(r => r.SearchAsync(query))
            .ReturnsAsync(matchingClients);

        var result = await _service.SearchAsync(query);

        Assert.Single(result);
        Assert.Contains(result, c => c.LastName == "Martin");
        _repositoryMock.Verify(r => r.SearchAsync(query), Times.Once);
    }
}
