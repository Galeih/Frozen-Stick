using Pierre.Web.Application.DTOs.Invoices;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;
using Pierre.Web.Domain.Exceptions;

namespace Pierre.Web.Application.Services;

public class InvoiceService
{
    private readonly IInvoiceRepository _repository;
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IInvoiceRepository repository,
        IClientRepository clientRepository,
        ILogger<InvoiceService> logger)
    {
        _repository = repository;
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task<List<InvoiceListItemDto>> GetAllAsync(InvoiceStatus? statusFilter = null)
    {
        var invoices = await _repository.GetAllAsync(statusFilter);
        return invoices.Select(MapToListItem).ToList();
    }

    public async Task<List<InvoiceListItemDto>> GetByClientIdAsync(Guid clientId)
    {
        var invoices = await _repository.GetByClientIdAsync(clientId);
        return invoices.Select(MapToListItem).ToList();
    }

    public async Task<InvoiceDetailDto> GetByIdAsync(Guid id)
    {
        var invoice = await _repository.GetByIdAsync(id);

        if (invoice == null)
        {
            throw new NotFoundException(nameof(Invoice), id);
        }

        return MapToDetail(invoice);
    }

    public async Task<InvoiceDetailDto> CreateAsync(CreateInvoiceDto dto)
    {
        var client = await _clientRepository.GetByIdAsync(dto.ClientId);

        if (client == null)
        {
            throw new NotFoundException(nameof(Client), dto.ClientId);
        }

        var now = DateTime.UtcNow;

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ClientId = dto.ClientId,
            Reference = dto.Reference.Trim(),
            Amount = dto.Amount,
            Status = Enum.Parse<InvoiceStatus>(dto.Status),
            IssuedAt = dto.IssuedAt,
            Notes = dto.Notes?.Trim()
        };

        await _repository.AddAsync(invoice);

        _logger.LogInformation("Invoice created: {Id} - {Reference} for client {ClientId}", invoice.Id, invoice.Reference, invoice.ClientId);

        return await GetByIdAsync(invoice.Id);
    }

    public async Task<InvoiceDetailDto> UpdateAsync(Guid id, UpdateInvoiceDto dto)
    {
        var invoice = await _repository.GetByIdAsync(id);

        if (invoice == null)
        {
            throw new NotFoundException(nameof(Invoice), id);
        }

        invoice.Reference = dto.Reference.Trim();
        invoice.Amount = dto.Amount;
        invoice.Status = Enum.Parse<InvoiceStatus>(dto.Status);
        invoice.IssuedAt = dto.IssuedAt;
        invoice.Notes = dto.Notes?.Trim();

        await _repository.UpdateAsync(invoice);

        _logger.LogInformation("Invoice updated: {Id} - {Reference}", invoice.Id, invoice.Reference);

        return await GetByIdAsync(invoice.Id);
    }

    public async Task UpdateStatusAsync(Guid id, InvoiceStatus status)
    {
        var invoice = await _repository.GetByIdAsync(id);

        if (invoice == null)
        {
            throw new NotFoundException(nameof(Invoice), id);
        }

        invoice.Status = status;
        await _repository.UpdateAsync(invoice);

        _logger.LogInformation("Invoice {Id} status updated to {Status}", id, status);
    }

    private static InvoiceListItemDto MapToListItem(Invoice invoice)
    {
        return new InvoiceListItemDto
        {
            Id = invoice.Id,
            Reference = invoice.Reference,
            ClientName = $"{invoice.Client.FirstName} {invoice.Client.LastName}",
            ClientId = invoice.ClientId,
            Amount = invoice.Amount,
            Status = invoice.Status.ToString(),
            IssuedAt = invoice.IssuedAt
        };
    }

    private static InvoiceDetailDto MapToDetail(Invoice invoice)
    {
        return new InvoiceDetailDto
        {
            Id = invoice.Id,
            ClientId = invoice.ClientId,
            ClientName = $"{invoice.Client.FirstName} {invoice.Client.LastName}",
            Reference = invoice.Reference,
            Amount = invoice.Amount,
            Status = invoice.Status.ToString(),
            IssuedAt = invoice.IssuedAt,
            Notes = invoice.Notes
        };
    }
}
