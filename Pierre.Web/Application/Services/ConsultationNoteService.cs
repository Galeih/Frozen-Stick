using Pierre.Web.Application.DTOs.ConsultationNotes;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Exceptions;
using Pierre.Web.Infrastructure.Data;

namespace Pierre.Web.Application.Services;

public class ConsultationNoteService
{
    private readonly IConsultationNoteRepository _repository;
    private readonly IClientRepository _clientRepository;
    private readonly AuditService _auditService;
    private readonly ILogger<ConsultationNoteService> _logger;

    public ConsultationNoteService(
        IConsultationNoteRepository repository,
        IClientRepository clientRepository,
        AuditService auditService,
        ILogger<ConsultationNoteService> logger)
    {
        _repository = repository;
        _clientRepository = clientRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<List<ConsultationNoteListItemDto>> GetByClientIdAsync(Guid clientId)
    {
        var notes = await _repository.GetByClientIdAsync(clientId);

        return notes.Select(n => new ConsultationNoteListItemDto
        {
            Id = n.Id,
            Date = n.Date,
            ContentPreview = n.Content.Length > 100
                ? n.Content[..100] + "..."
                : n.Content,
            Weight = n.Weight,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task<ConsultationNoteDetailDto> GetByIdAsync(Guid id)
    {
        var note = await _repository.GetByIdAsync(id);

        if (note == null)
        {
            throw new NotFoundException(nameof(ConsultationNote), id);
        }

        return MapToDetail(note);
    }

    public async Task<ConsultationNoteDetailDto> CreateAsync(CreateConsultationNoteDto dto)
    {
        var client = await _clientRepository.GetByIdAsync(dto.ClientId);

        if (client == null)
        {
            throw new NotFoundException(nameof(Client), dto.ClientId);
        }

        var now = DateTime.UtcNow;

        var note = new ConsultationNote
        {
            Id = Guid.NewGuid(),
            ClientId = dto.ClientId,
            AppointmentId = dto.AppointmentId,
            Date = dto.Date,
            Content = dto.Content.Trim(),
            Recommendations = dto.Recommendations?.Trim(),
            Weight = dto.Weight,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddAsync(note);

        _logger.LogInformation(
            "Consultation note created: {Id} for client {ClientId}",
            note.Id, note.ClientId);
        _auditService.Log("Note de consultation créée", $"Client {note.ClientId} (Note ID: {note.Id})");

        return await GetByIdAsync(note.Id);
    }

    public async Task<ConsultationNoteDetailDto> UpdateAsync(Guid id, UpdateConsultationNoteDto dto)
    {
        var note = await _repository.GetByIdAsync(id);

        if (note == null)
        {
            throw new NotFoundException(nameof(ConsultationNote), id);
        }

        note.Date = dto.Date;
        note.Content = dto.Content.Trim();
        note.Recommendations = dto.Recommendations?.Trim();
        note.Weight = dto.Weight;
        note.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(note);

        _logger.LogInformation(
            "Consultation note updated: {Id} for client {ClientId}",
            note.Id, note.ClientId);
        _auditService.Log("Note de consultation modifiée", $"Client {note.ClientId} (Note ID: {note.Id})");

        return await GetByIdAsync(note.Id);
    }

    private static ConsultationNoteDetailDto MapToDetail(ConsultationNote note)
    {
        return new ConsultationNoteDetailDto
        {
            Id = note.Id,
            ClientId = note.ClientId,
            ClientName = note.Client != null
                ? $"{note.Client.FirstName} {note.Client.LastName}"
                : string.Empty,
            AppointmentId = note.AppointmentId,
            AppointmentInfo = note.Appointment?.Slot != null
                ? $"{note.Appointment.Slot.Date:dd/MM/yyyy} à {note.Appointment.Slot.StartTime:HH\\hmm}"
                : null,
            Date = note.Date,
            Content = note.Content,
            Recommendations = note.Recommendations,
            Weight = note.Weight,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }
}
