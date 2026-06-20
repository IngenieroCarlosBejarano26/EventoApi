using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Events.Shared;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Features.Events.CreateEvent;

public sealed record CreateEventCommand(
    string Title,
    string Description,
    int VenueId,
    int MaxCapacity,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Price,
    EventType Type) : ICommand<EventDto>;
