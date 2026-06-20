using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Common.Abstractions;

public sealed record EventFilter(
    EventType? Type = null,
    DateTimeOffset? StartDateFrom = null,
    DateTimeOffset? StartDateTo = null,
    int? VenueId = null,
    EventStatus? Status = null,
    string? TitleContains = null);
