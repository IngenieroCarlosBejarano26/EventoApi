using Microsoft.AspNetCore.SignalR;

namespace EventosVivos.Api.RealTime;

/// <summary>
/// Hub de eventos en tiempo real. Solo difunde (server -> client); los clientes se suscriben
/// para recibir actualizaciones de disponibilidad ("EventUpdated").
/// </summary>
public sealed class EventsHub : Hub;
