using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Events.Shared;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Features.Events.GetEvents;

/// <summary>RF-02: Listar eventos con filtros opcionales.</summary>
public sealed record GetEventsQuery(
    EventType? Type = null,
    DateTimeOffset? StartDateFrom = null,
    DateTimeOffset? StartDateTo = null,
    int? VenueId = null,
    EventStatus? Status = null,
    string? Title = null) : IQuery<IReadOnlyList<EventDto>>;
