using Pierre.Web.Application.DTOs;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Application.Services;

public class DashboardService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IContentRepository _contentRepository;

    public DashboardService(
        IAppointmentRepository appointmentRepository,
        IClientRepository clientRepository,
        IContentRepository contentRepository)
    {
        _appointmentRepository = appointmentRepository;
        _clientRepository = clientRepository;
        _contentRepository = contentRepository;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var upcomingAppointments = await _appointmentRepository.GetUpcomingAcceptedAsync(5);
        var pendingAppointments = await _appointmentRepository.GetPendingAsync();
        var recentClients = await _clientRepository.GetAllActiveAsync();
        var recentContents = await _contentRepository.GetPublishedAsync();

        var dto = new DashboardDto
        {
            UpcomingAppointments = upcomingAppointments
                .Select(a => new DashboardAppointmentDto
                {
                    Id = a.Id,
                    RequesterName = a.RequesterName,
                    Date = a.Slot.Date,
                    StartTime = a.Slot.StartTime,
                    CreatedAt = a.CreatedAt
                }).ToList(),

            PendingRequestsCount = pendingAppointments.Count,

            RecentPendingRequests = pendingAppointments
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new DashboardAppointmentDto
                {
                    Id = a.Id,
                    RequesterName = a.RequesterName,
                    Date = a.Slot.Date,
                    StartTime = a.Slot.StartTime,
                    CreatedAt = a.CreatedAt
                }).ToList(),

            RecentClients = recentClients
                .OrderByDescending(c => c.CreatedAt)
                .Take(3)
                .Select(c => new DashboardClientDto
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    CreatedAt = c.CreatedAt
                }).ToList(),

            RecentContents = recentContents
                .OrderByDescending(c => c.PublishedAt)
                .Take(3)
                .Select(c => new DashboardContentDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Slug = c.Slug,
                    Type = c.Type.ToString(),
                    PublishedAt = c.PublishedAt
                }).ToList()
        };

        return dto;
    }
}
