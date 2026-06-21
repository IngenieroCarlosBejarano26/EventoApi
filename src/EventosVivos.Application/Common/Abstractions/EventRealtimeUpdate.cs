namespace EventosVivos.Application.Common.Abstractions;

public sealed record EventRealtimeUpdate(
    Guid EventId,
    int AvailableTickets,
    int MaxCapacity,
    int SoldTickets,
    string Status);
