using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Features.Events.Shared;

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
