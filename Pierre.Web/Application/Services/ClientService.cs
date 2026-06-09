using Pierre.Web.Application.DTOs.Clients;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Application.Services;

public class ClientService
{
    private readonly IClientRepository _repository;
    private readonly ILogger<ClientService> _logger;

    public ClientService(IClientRepository repository, ILogger<ClientService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<ClientListItemDto>> GetAllAsync(bool includeArchived = false)
    {
        List<Client> clients;

        if (includeArchived)
        {
            clients = await _repository.GetAllIncludingArchivedAsync();
        }
        else
        {
            clients = await _repository.GetAllActiveAsync();
        }

        return clients.Select(MapToListItem).ToList();
    }

    public async Task<List<ClientListItemDto>> SearchAsync(string query)
    {
        var clients = await _repository.SearchAsync(query);
        return clients.Select(MapToListItem).ToList();
    }

    public async Task<ClientDetailDto> GetByIdAsync(Guid id)
    {
        var client = await _repository.GetByIdAsync(id);

        if (client == null)
        {
            throw new NotFoundException(nameof(Client), id);
        }

        return MapToDetail(client);
    }

    public async Task<ClientDetailDto> CreateAsync(CreateClientDto dto)
    {
        var now = DateTime.UtcNow;

        var client = new Client
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email?.Trim(),
            Phone = dto.Phone?.Trim(),
            BirthDate = dto.BirthDate,
            Notes = dto.Notes?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddAsync(client);

        _logger.LogInformation("Client created: {Id} - {FirstName} {LastName}", client.Id, client.FirstName, client.LastName);

        return await GetByIdAsync(client.Id);
    }

    public async Task<ClientDetailDto> UpdateAsync(Guid id, UpdateClientDto dto)
    {
        var client = await _repository.GetByIdAsync(id);

        if (client == null)
        {
            throw new NotFoundException(nameof(Client), id);
        }

        client.FirstName = dto.FirstName.Trim();
        client.LastName = dto.LastName.Trim();
        client.Email = dto.Email?.Trim();
        client.Phone = dto.Phone?.Trim();
        client.BirthDate = dto.BirthDate;
        client.Notes = dto.Notes?.Trim();
        client.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(client);

        _logger.LogInformation("Client updated: {Id} - {FirstName} {LastName}", client.Id, client.FirstName, client.LastName);

        return await GetByIdAsync(client.Id);
    }

    public async Task ArchiveAsync(Guid id)
    {
        var client = await _repository.GetByIdAsync(id);

        if (client == null)
        {
            throw new NotFoundException(nameof(Client), id);
        }

        await _repository.ArchiveAsync(id);

        _logger.LogInformation("Client archived: {Id} - {FirstName} {LastName}", id, client.FirstName, client.LastName);
    }

    private static ClientListItemDto MapToListItem(Client client)
    {
        return new ClientListItemDto
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Phone = client.Phone,
            CreatedAt = client.CreatedAt,
            IsArchived = client.IsArchived
        };
    }

    private static ClientDetailDto MapToDetail(Client client)
    {
        return new ClientDetailDto
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Phone = client.Phone,
            BirthDate = client.BirthDate,
            Notes = client.Notes,
            IsArchived = client.IsArchived,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt,
            AppointmentCount = client.Appointments?.Count ?? 0,
            Appointments = (client.Appointments ?? new List<Appointment>())
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new ClientAppointmentDto
                {
                    Id = a.Id,
                    Date = a.Slot?.Date ?? default,
                    StartTime = a.Slot?.StartTime ?? default,
                    EndTime = a.Slot?.EndTime ?? default,
                    Status = a.Status.ToString(),
                    CreatedAt = a.CreatedAt
                }).ToList(),
            ConsultationNotes = (client.ConsultationNotes ?? new List<ConsultationNote>())
                .OrderByDescending(n => n.Date)
                .Select(n => new ClientNoteDto
                {
                    Id = n.Id,
                    Date = n.Date,
                    Content = n.Content,
                    Recommendations = n.Recommendations,
                    Weight = n.Weight
                }).ToList()
        };
    }
}
