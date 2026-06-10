using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pierre.Web.Application.DTOs.Appointments;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Application.Services;
using Pierre.Web.Configuration;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Tests;

public class AppointmentServiceTests
{
    private readonly Mock<IAvailabilityRepository> _availabilityRepoMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IOptions<AdminSeedSettings>> _adminSettingsMock;
    private readonly Mock<ILogger<AppointmentService>> _loggerMock;
    private readonly AppointmentService _service;
    private static readonly AuditService AuditServiceStub = new(Mock.Of<ILogger<AuditService>>());

    public AppointmentServiceTests()
    {
        _availabilityRepoMock = new Mock<IAvailabilityRepository>();
        _appointmentRepoMock = new Mock<IAppointmentRepository>();
        _clientRepoMock = new Mock<IClientRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _adminSettingsMock = new Mock<IOptions<AdminSeedSettings>>();
        _loggerMock = new Mock<ILogger<AppointmentService>>();

        _adminSettingsMock
            .Setup(s => s.Value)
            .Returns(new AdminSeedSettings
            {
                Email = "admin@pierre-dieteticien.fr",
                Password = "Admin123!",
                FirstName = "Pierre",
                LastName = "Admin"
            });

        _emailServiceMock
            .Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _service = new AppointmentService(
            _availabilityRepoMock.Object,
            _appointmentRepoMock.Object,
            _clientRepoMock.Object,
            _emailServiceMock.Object,
            _adminSettingsMock.Object,
            AuditServiceStub,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RequestAppointmentAsync_WithValidData_ShouldCreatePendingAppointmentAndSendEmail()
    {
        var slotId = Guid.NewGuid();
        var dto = new AppointmentRequestDto
        {
            SlotId = slotId,
            FirstName = "Jean",
            LastName = "Dupont",
            Email = "jean.dupont@exemple.fr",
            Phone = "0612345678",
            Message = "Je souhaite un bilan nutritionnel."
        };

        _appointmentRepoMock
            .Setup(r => r.GetBySlotIdAsync(slotId))
            .ReturnsAsync((Appointment?)null);

        Appointment? savedAppointment = null;

        _appointmentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Appointment>()))
            .Callback<Appointment>(a => savedAppointment = a)
            .Returns(Task.CompletedTask);

        await _service.RequestAppointmentAsync(dto);

        Assert.NotNull(savedAppointment);
        Assert.Equal("Jean Dupont", savedAppointment.RequesterName);
        Assert.Equal("jean.dupont@exemple.fr", savedAppointment.RequesterEmail);
        Assert.Equal("0612345678", savedAppointment.RequesterPhone);
        Assert.Equal("Je souhaite un bilan nutritionnel.", savedAppointment.Message);
        Assert.Equal(AppointmentStatus.Pending, savedAppointment.Status);
        Assert.Equal(slotId, savedAppointment.SlotId);

        _emailServiceMock.Verify(
            e => e.SendAsync(
                "admin@pierre-dieteticien.fr",
                It.Is<string>(s => s.Contains("Nouvelle demande")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestAppointmentAsync_WithoutEmailAndPhone_ShouldThrowValidationException()
    {
        var dto = new AppointmentRequestDto
        {
            SlotId = Guid.NewGuid(),
            FirstName = "Jean",
            LastName = "Dupont",
            Email = null,
            Phone = null
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _service.RequestAppointmentAsync(dto));

        Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("téléphone", ex.Message, StringComparison.OrdinalIgnoreCase);

        _appointmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Never);
        _emailServiceMock.Verify(
            e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_ShouldNotReturnBlockedSlots()
    {
        var now = DateTime.UtcNow;
        var freeSlotId = Guid.NewGuid();

        var freeSlot = new Availability
        {
            Id = freeSlotId,
            Date = DateOnly.FromDateTime(now.AddDays(1)),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsBlocked = false,
            Appointment = null
        };

        _availabilityRepoMock
            .Setup(r => r.GetAvailableSlotsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync((DateTime from, DateTime to) =>
            {
                var fromDate = DateOnly.FromDateTime(from);
                var toDate = DateOnly.FromDateTime(to);

                Assert.True(fromDate <= DateOnly.FromDateTime(now));
                Assert.True(toDate >= DateOnly.FromDateTime(now.AddDays(28)));

                return new List<Availability> { freeSlot };
            });

        var result = await _service.GetAvailableSlotsAsync();

        Assert.Single(result);
        Assert.Equal(freeSlotId, result[0].SlotId);
        Assert.Equal(new TimeOnly(9, 0), result[0].StartTime);
        Assert.Equal(new TimeOnly(10, 0), result[0].EndTime);
    }

    [Fact]
    public async Task AcceptAsync_ShouldBlockSlotAndUpdateStatus()
    {
        var appointmentId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        var slot = new Availability
        {
            Id = slotId,
            Date = new DateOnly(2026, 6, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsBlocked = false
        };

        var appointment = new Appointment
        {
            Id = appointmentId,
            SlotId = slotId,
            Slot = slot,
            RequesterName = "Jean Dupont",
            RequesterEmail = "jean@exemple.fr",
            RequesterPhone = "0612345678",
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        _availabilityRepoMock
            .Setup(r => r.GetByIdAsync(slotId))
            .ReturnsAsync(slot);

        _clientRepoMock
            .Setup(r => r.FindByContactAsync("jean@exemple.fr", "0612345678"))
            .ReturnsAsync((Client?)null);

        Client? createdClient = null;

        _clientRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Client>()))
            .Callback<Client>(c => createdClient = c)
            .Returns(Task.CompletedTask);

        await _service.AcceptAsync(appointmentId);

        Assert.Equal(AppointmentStatus.Accepted, appointment.Status);
        Assert.True(slot.IsBlocked);
        Assert.NotNull(appointment.ClientId);

        Assert.NotNull(createdClient);
        Assert.Equal("Jean", createdClient.FirstName);
        Assert.Equal("Dupont", createdClient.LastName);
        Assert.Equal("jean@exemple.fr", createdClient.Email);
        Assert.Equal("0612345678", createdClient.Phone);

        _emailServiceMock.Verify(
            e => e.SendAsync(
                "jean@exemple.fr",
                It.Is<string>(s => s.Contains("Confirmation")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_ShouldLinkExistingClient_WhenEmailAlreadyExists()
    {
        var appointmentId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        var slot = new Availability
        {
            Id = slotId,
            Date = new DateOnly(2026, 6, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsBlocked = false
        };

        var appointment = new Appointment
        {
            Id = appointmentId,
            SlotId = slotId,
            Slot = slot,
            RequesterName = "Jean Dupont",
            RequesterEmail = "jean@exemple.fr",
            RequesterPhone = "0612345678",
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var existingClient = new Client
        {
            Id = Guid.NewGuid(),
            FirstName = "Jean",
            LastName = "Dupont",
            Email = "jean@exemple.fr",
            Phone = "0612345678"
        };

        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        _availabilityRepoMock
            .Setup(r => r.GetByIdAsync(slotId))
            .ReturnsAsync(slot);

        _clientRepoMock
            .Setup(r => r.FindByContactAsync("jean@exemple.fr", "0612345678"))
            .ReturnsAsync(existingClient);

        await _service.AcceptAsync(appointmentId);

        Assert.Equal(AppointmentStatus.Accepted, appointment.Status);
        Assert.True(slot.IsBlocked);
        Assert.Equal(existingClient.Id, appointment.ClientId);

        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task RefuseAsync_ShouldNotBlockSlot()
    {
        var appointmentId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        var slot = new Availability
        {
            Id = slotId,
            Date = new DateOnly(2026, 6, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsBlocked = false
        };

        var appointment = new Appointment
        {
            Id = appointmentId,
            SlotId = slotId,
            Slot = slot,
            RequesterName = "Jean Dupont",
            RequesterEmail = "jean@exemple.fr",
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        await _service.RefuseAsync(appointmentId);

        Assert.Equal(AppointmentStatus.Refused, appointment.Status);
        Assert.False(slot.IsBlocked);

        _availabilityRepoMock.Verify(
            r => r.UpdateAsync(It.IsAny<Availability>()),
            Times.Never);

        _emailServiceMock.Verify(
            e => e.SendAsync(
                "jean@exemple.fr",
                It.Is<string>(s => s.Contains("refus")),
                It.IsAny<string>()),
            Times.Once);
    }
}
