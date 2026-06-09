using Microsoft.Extensions.Options;
using Pierre.Web.Application.DTOs.Appointments;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Configuration;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Application.Services;

public class AppointmentService
{
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IEmailService _emailService;
    private readonly IOptions<AdminSeedSettings> _adminSettings;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAvailabilityRepository availabilityRepository,
        IAppointmentRepository appointmentRepository,
        IClientRepository clientRepository,
        IEmailService emailService,
        IOptions<AdminSeedSettings> adminSettings,
        ILogger<AppointmentService> logger)
    {
        _availabilityRepository = availabilityRepository;
        _appointmentRepository = appointmentRepository;
        _clientRepository = clientRepository;
        _emailService = emailService;
        _adminSettings = adminSettings;
        _logger = logger;
    }

    public async Task<List<AvailabilitySlotDto>> GetAvailableSlotsAsync()
    {
        var now = DateTime.UtcNow;
        var from = now;
        var to = now.AddDays(28);

        var slots = await _availabilityRepository.GetAvailableSlotsAsync(from, to);

        return slots.Select(s => new AvailabilitySlotDto
        {
            SlotId = s.Id,
            Date = s.Date,
            StartTime = s.StartTime,
            EndTime = s.EndTime
        }).ToList();
    }

    public async Task RequestAppointmentAsync(AppointmentRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.Phone))
        {
            throw new ValidationException("Vous devez fournir au moins un email ou un numéro de téléphone.");
        }

        var existing = await _appointmentRepository.GetBySlotIdAsync(dto.SlotId);

        if (existing != null)
        {
            throw new ConflictException("Ce créneau n'est plus disponible.");
        }

        var now = DateTime.UtcNow;

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            SlotId = dto.SlotId,
            RequesterName = $"{dto.FirstName.Trim()} {dto.LastName.Trim()}",
            RequesterEmail = dto.Email?.Trim(),
            RequesterPhone = dto.Phone?.Trim(),
            Message = dto.Message?.Trim(),
            Status = AppointmentStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _appointmentRepository.AddAsync(appointment);

        _logger.LogInformation(
            "Appointment requested: {Name}, Slot {SlotId}",
            appointment.RequesterName, appointment.SlotId);

        await NotifyProfessionalAsync(appointment);
    }

    public async Task<List<AppointmentListItemDto>> GetPendingAsync()
    {
        var appointments = await _appointmentRepository.GetPendingAsync();
        return appointments.Select(MapToListItem).ToList();
    }

    public async Task<List<AppointmentListItemDto>> GetAllForAdminAsync(AppointmentStatus? statusFilter = null)
    {
        var appointments = await _appointmentRepository.GetAllAsync(statusFilter);
        return appointments.Select(MapToListItem).ToList();
    }

    public async Task<AppointmentDetailDto> GetByIdAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);

        if (appointment == null)
        {
            throw new NotFoundException(nameof(Appointment), id);
        }

        return MapToDetail(appointment);
    }

    public async Task AcceptAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);

        if (appointment == null)
        {
            throw new NotFoundException(nameof(Appointment), id);
        }

        if (appointment.Status != AppointmentStatus.Pending)
        {
            throw new ValidationException("Seules les demandes en attente peuvent être acceptées.");
        }

        var now = DateTime.UtcNow;

        var existingClient = await _clientRepository.FindByContactAsync(
            appointment.RequesterEmail, appointment.RequesterPhone);

        Client client;

        if (existingClient != null)
        {
            client = existingClient;
            _logger.LogInformation("Found existing client {ClientId} for {Email}", client.Id, appointment.RequesterEmail);
        }
        else
        {
            client = new Client
            {
                Id = Guid.NewGuid(),
                FirstName = ExtractFirstName(appointment.RequesterName),
                LastName = ExtractLastName(appointment.RequesterName),
                Email = appointment.RequesterEmail,
                Phone = appointment.RequesterPhone,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _clientRepository.AddAsync(client);
            _logger.LogInformation("Created new client {ClientId} for {Name}", client.Id, appointment.RequesterName);
        }

        appointment.ClientId = client.Id;
        appointment.Status = AppointmentStatus.Accepted;
        appointment.UpdatedAt = now;

        await _appointmentRepository.UpdateAsync(appointment);

        var slot = await _availabilityRepository.GetByIdAsync(appointment.SlotId);

        if (slot != null)
        {
            slot.IsBlocked = true;
            await _availabilityRepository.UpdateAsync(slot);
        }

        _logger.LogInformation("Appointment {Id} accepted for {Name}", appointment.Id, appointment.RequesterName);

        if (!string.IsNullOrWhiteSpace(appointment.RequesterEmail))
        {
            var body = $@"Bonjour {appointment.RequesterName},

Votre rendez-vous du {appointment.Slot.Date:dddd dd MMMM yyyy} à {appointment.Slot.StartTime:HH\hmm} a été confirmé.

Nous vous recevrons avec plaisir à l'adresse indiquée sur notre site.

Si vous avez des questions, n'hésitez pas à nous contacter.

Cordialement,
Pierre Diététicien";

            await _emailService.SendAsync(
                appointment.RequesterEmail,
                "[Pierre Diététicien] Confirmation de rendez-vous",
                body);
        }
    }

    public async Task RefuseAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);

        if (appointment == null)
        {
            throw new NotFoundException(nameof(Appointment), id);
        }

        if (appointment.Status != AppointmentStatus.Pending)
        {
            throw new ValidationException("Seules les demandes en attente peuvent être refusées.");
        }

        appointment.Status = AppointmentStatus.Refused;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);

        _logger.LogInformation("Appointment {Id} refused for {Name}", appointment.Id, appointment.RequesterName);

        if (!string.IsNullOrWhiteSpace(appointment.RequesterEmail))
        {
            var body = $@"Bonjour {appointment.RequesterName},

Nous sommes au regret de vous informer que votre rendez-vous du {appointment.Slot.Date:dddd dd MMMM yyyy} à {appointment.Slot.StartTime:HH\hmm} n'a pas pu être accepté.

Vous pouvez consulter notre planning en ligne pour choisir un autre créneau.

Cordialement,
Pierre Diététicien";

            await _emailService.SendAsync(
                appointment.RequesterEmail,
                "[Pierre Diététicien] Demande de rendez-vous refusée",
                body);
        }
    }

    private async Task NotifyProfessionalAsync(Appointment appointment)
    {
        var adminEmail = _adminSettings.Value.Email;

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            _logger.LogWarning("Admin email not configured, skipping notification");
            return;
        }

        var body = $@"Nouvelle demande de rendez-vous

Un visiteur a demandé un rendez-vous sur le site.

Détails de la demande :
  Nom : {appointment.RequesterName}
  Email : {appointment.RequesterEmail ?? "Non renseigné"}
  Téléphone : {appointment.RequesterPhone ?? "Non renseigné"}
  Message : {appointment.Message ?? "Aucun message"}

Connectez-vous à l'interface d'administration pour accepter ou refuser cette demande.";

        await _emailService.SendAsync(
            adminEmail,
            "[Pierre Diététicien] Nouvelle demande de rendez-vous",
            body);
    }

    private static AppointmentListItemDto MapToListItem(Appointment a)
    {
        return new AppointmentListItemDto
        {
            Id = a.Id,
            RequesterName = a.RequesterName,
            Date = a.Slot.Date,
            StartTime = a.Slot.StartTime,
            Status = a.Status.ToString(),
            StatusDisplay = GetStatusDisplayName(a.Status),
            ClientName = a.Client != null ? $"{a.Client.FirstName} {a.Client.LastName}" : null,
            CreatedAt = a.CreatedAt
        };
    }

    private static AppointmentDetailDto MapToDetail(Appointment a)
    {
        return new AppointmentDetailDto
        {
            Id = a.Id,
            SlotId = a.SlotId,
            Date = a.Slot.Date,
            StartTime = a.Slot.StartTime,
            EndTime = a.Slot.EndTime,
            RequesterName = a.RequesterName,
            RequesterEmail = a.RequesterEmail,
            RequesterPhone = a.RequesterPhone,
            Message = a.Message,
            Status = a.Status.ToString(),
            StatusDisplay = GetStatusDisplayName(a.Status),
            ClientId = a.ClientId,
            ClientName = a.Client != null ? $"{a.Client.FirstName} {a.Client.LastName}" : null,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        };
    }

    private static string GetStatusDisplayName(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Pending => "En attente",
        AppointmentStatus.Accepted => "Accepté",
        AppointmentStatus.Refused => "Refusé",
        AppointmentStatus.Cancelled => "Annulé",
        _ => status.ToString()
    };

    private static string ExtractFirstName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : fullName;
    }

    private static string ExtractLastName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? string.Join(' ', parts[1..]) : fullName;
    }
}
