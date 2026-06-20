using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Common.Abstractions;

/// <summary>Criterios opcionales de filtrado para el listado de eventos (RF-02).</summary>
public sealed record EventFilter(
    EventType? Type = null,
    DateTimeOffset? StartDateFrom = null,
    DateTimeOffset? StartDateTo = null,
    int? VenueId = null,
    EventStatus? Status = null,
    string? TitleContains = null);
