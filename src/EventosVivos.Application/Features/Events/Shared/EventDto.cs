using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Features.Events.Shared;

public sealed record EventDto(
    Guid Id,
    string Title,
    string Description,
    int VenueId,
    string VenueName,
    int MaxCapacity,
    int AvailableTickets,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Price,
    EventType Type,
    EventStatus Status);
