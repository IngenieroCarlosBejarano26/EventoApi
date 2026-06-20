using EventosVivos.Application.Features.Events.Shared;

namespace EventosVivos.Application.Common.Abstractions;

/// <summary>
/// Puerto de salida para notificaciones en tiempo real. El adaptador concreto (SignalR, WebSockets,
/// un message broker, etc.) vive en una capa externa, manteniendo el núcleo agnóstico al transporte.
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>Difunde el cambio de disponibilidad/estado de un evento a los clientes conectados.</summary>
    Task EventUpdatedAsync(EventRealtimeUpdate update, CancellationToken cancellationToken = default);

    /// <summary>Difunde un evento recién creado para que aparezca en vivo en los listados.</summary>
    Task EventCreatedAsync(EventDto @event, CancellationToken cancellationToken = default);
}

/// <summary>Instantánea ligera del evento para refrescar la UI en vivo (evita la sobreventa visible).</summary>
public sealed record EventRealtimeUpdate(
    Guid EventId,
    int AvailableTickets,
    int MaxCapacity,
    int SoldTickets,
    string Status);
