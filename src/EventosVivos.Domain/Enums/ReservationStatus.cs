namespace EventosVivos.Domain.Enums;

/// <summary>
/// Estado del ciclo de vida de una reserva.
/// Perdida = cancelación con penalización (RN07): no libera entradas, solo cuenta para reporte.
/// </summary>
public enum ReservationStatus
{
    PendientePago = 1,
    Confirmada = 2,
    Cancelada = 3,
    Perdida = 4
}
