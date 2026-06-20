using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Events.Shared;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Features.Events.GetEvents;

public sealed record GetEventsQuery(
    EventType? Type = null,
    DateTimeOffset? StartDateFrom = null,
    DateTimeOffset? StartDateTo = null,
    int? VenueId = null,
    EventStatus? Status = null,
    string? Title = null) : IQuery<IReadOnlyList<EventDto>>;
