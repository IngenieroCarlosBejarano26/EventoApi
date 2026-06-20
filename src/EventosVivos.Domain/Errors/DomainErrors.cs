using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Errors;

/// <summary>
/// Catálogo central de errores de negocio. Tener los errores como datos con código estable
/// facilita el testing, la trazabilidad y el mapeo a respuestas ProblemDetails.
/// </summary>
public static class DomainErrors
{
    public static class Email
    {
        public static readonly Error Empty =
            Error.Validation("Email.Empty", "El email es obligatorio.");
        public static readonly Error TooLong =
            Error.Validation("Email.TooLong", "El email no puede superar 256 caracteres.");
        public static readonly Error Invalid =
            Error.Validation("Email.Invalid", "El formato del email no es válido.");
    }

    public static class Venue
    {
        public static Error NotFound(int id) =>
            Error.NotFound("Venue.NotFound", $"No existe el venue con id {id}.");
    }

    public static class Event
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound("Event.NotFound", $"No existe el evento con id {id}.");

        public static readonly Error TitleInvalid =
            Error.Validation("Event.TitleInvalid", "El título debe tener entre 5 y 100 caracteres.");
        public static readonly Error DescriptionInvalid =
            Error.Validation("Event.DescriptionInvalid", "La descripción debe tener entre 10 y 500 caracteres.");
        public static readonly Error CapacityNotPositive =
            Error.Validation("Event.CapacityNotPositive", "La capacidad máxima debe ser un entero positivo.");
        public static readonly Error PriceNotPositive =
            Error.Validation("Event.PriceNotPositive", "El precio debe ser un decimal positivo.");
        public static readonly Error StartMustBeFuture =
            Error.Validation("Event.StartMustBeFuture", "La fecha de inicio debe ser futura.");
        public static readonly Error EndMustBeAfterStart =
            Error.Validation("Event.EndMustBeAfterStart", "La fecha de fin debe ser posterior al inicio.");

        // RN01
        public static readonly Error ExceedsVenueCapacity =
            Error.Conflict("Event.ExceedsVenueCapacity", "La capacidad del evento excede la capacidad del venue.");
        // RN02
        public static readonly Error VenueOverlap =
            Error.Conflict("Event.VenueOverlap", "Ya existe un evento activo en el mismo venue con horario superpuesto.");
        // RN03
        public static readonly Error WeekendNightRestriction =
            Error.Conflict("Event.WeekendNightRestriction", "Los eventos en fin de semana no pueden iniciar después de las 22:00.");
    }

    public static class Reservation
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound("Reservation.NotFound", $"No existe la reserva con id {id}.");

        public static readonly Error EventNotActive =
            Error.Conflict("Reservation.EventNotActive", "Solo se pueden reservar entradas de eventos activos.");
        public static readonly Error InvalidQuantity =
            Error.Validation("Reservation.InvalidQuantity", "La cantidad debe ser 1 o más.");
        public static readonly Error NotEnoughTickets =
            Error.Conflict("Reservation.NotEnoughTickets", "No hay suficientes entradas disponibles.");
        // RN04
        public static readonly Error TooLate =
            Error.Conflict("Reservation.TooLate", "No se permiten reservas para eventos que inician en menos de 1 hora.");
        // RF-03 (menos de 24h -> máx 5)
        public static readonly Error MaxFiveWithin24Hours =
            Error.Conflict("Reservation.MaxFiveWithin24Hours", "Faltan menos de 24h: máximo 5 entradas por transacción.");
        // RN05 (precio > 100 -> máx 10)
        public static readonly Error MaxTenForPremium =
            Error.Conflict("Reservation.MaxTenForPremium", "Eventos con precio mayor a $100 limitan a 10 entradas por transacción.");

        public static readonly Error AlreadyConfirmed =
            Error.Conflict("Reservation.AlreadyConfirmed", "La reserva ya está confirmada.");
        public static readonly Error CannotConfirmCancelled =
            Error.Conflict("Reservation.CannotConfirmCancelled", "No se puede confirmar una reserva cancelada o perdida.");
        public static readonly Error AlreadyCancelled =
            Error.Conflict("Reservation.AlreadyCancelled", "La reserva ya está cancelada o perdida.");
    }
}
