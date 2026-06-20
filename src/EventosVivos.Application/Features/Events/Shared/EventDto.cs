using EventosVivos.Domain.Entities;
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

public static class EventMappingExtensions
{
    public static EventDto ToDto(this Event @event) => new(
        @event.Id,
        @event.Title,
        @event.Description,
        @event.VenueId,
        @event.Venue?.Name ?? string.Empty,
        @event.MaxCapacity,
        @event.AvailableTickets,
        @event.StartDate,
        @event.EndDate,
        @event.Price,
        @event.Type,
        @event.Status);
}
