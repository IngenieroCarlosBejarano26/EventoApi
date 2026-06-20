using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Errors;

public static class DomainErrors
{
    public static class Email
    {
        public static readonly Error Empty =
            Error.Validation("Email.Empty", "El email es obligatorio.");
        public static readonly Error TooLong =
            Error.Validation("Email.TooLong", "El email no puede superar 256 caracteres.");
        public static readonly Error Invalid =
            Error.Validation("Email.Invalid", "El formato del email no es vÃ¡lido.");
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
            Error.Validation("Event.TitleInvalid", "El tÃ­tulo debe tener entre 5 y 100 caracteres.");
        public static readonly Error DescriptionInvalid =
            Error.Validation("Event.DescriptionInvalid", "La descripciÃ³n debe tener entre 10 y 500 caracteres.");
        public static readonly Error CapacityNotPositive =
            Error.Validation("Event.CapacityNotPositive", "La capacidad mÃ¡xima debe ser un entero positivo.");
        public static readonly Error PriceNotPositive =
            Error.Validation("Event.PriceNotPositive", "El precio debe ser un decimal positivo.");
        public static readonly Error StartMustBeFuture =
            Error.Validation("Event.StartMustBeFuture", "La fecha de inicio debe ser futura.");
        public static readonly Error EndMustBeAfterStart =
            Error.Validation("Event.EndMustBeAfterStart", "La fecha de fin debe ser posterior al inicio.");

        public static readonly Error ExceedsVenueCapacity =
            Error.Conflict("Event.ExceedsVenueCapacity", "La capacidad del evento excede la capacidad del venue.");
        public static readonly Error VenueOverlap =
            Error.Conflict("Event.VenueOverlap", "Ya existe un evento activo en el mismo venue con horario superpuesto.");
        public static readonly Error WeekendNightRestriction =
            Error.Conflict("Event.WeekendNightRestriction", "Los eventos en fin de semana no pueden iniciar despuÃ©s de las 22:00.");
    }

    public static class Reservation
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound("Reservation.NotFound", $"No existe la reserva con id {id}.");

        public static readonly Error EventNotActive =
            Error.Conflict("Reservation.EventNotActive", "Solo se pueden reservar entradas de eventos activos.");
        public static readonly Error InvalidQuantity =
            Error.Validation("Reservation.InvalidQuantity", "La cantidad debe ser 1 o mÃ¡s.");
        public static readonly Error NotEnoughTickets =
            Error.Conflict("Reservation.NotEnoughTickets", "No hay suficientes entradas disponibles.");
        public static readonly Error TooLate =
            Error.Conflict("Reservation.TooLate", "No se permiten reservas para eventos que inician en menos de 1 hora.");
        public static readonly Error MaxFiveWithin24Hours =
            Error.Conflict("Reservation.MaxFiveWithin24Hours", "Faltan menos de 24h: mÃ¡ximo 5 entradas por transacciÃ³n.");
        public static readonly Error MaxTenForPremium =
            Error.Conflict("Reservation.MaxTenForPremium", "Eventos con precio mayor a $100 limitan a 10 entradas por transacciÃ³n.");

        public static readonly Error AlreadyConfirmed =
            Error.Conflict("Reservation.AlreadyConfirmed", "La reserva ya estÃ¡ confirmada.");
        public static readonly Error CannotConfirmCancelled =
            Error.Conflict("Reservation.CannotConfirmCancelled", "No se puede confirmar una reserva cancelada o perdida.");
        public static readonly Error AlreadyCancelled =
            Error.Conflict("Reservation.AlreadyCancelled", "La reserva ya estÃ¡ cancelada o perdida.");
    }
}
