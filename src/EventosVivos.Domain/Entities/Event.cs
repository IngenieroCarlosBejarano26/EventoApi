using EventosVivos.Domain.Common;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Errors;
using EventosVivos.Domain.Events;

namespace EventosVivos.Domain.Entities;

/// <summary>
/// Raíz de agregado Evento. Concentra las invariantes de negocio relacionadas con el evento
/// y la disponibilidad de entradas en tiempo real (clave para evitar la sobreventa).
/// </summary>
public sealed class Event : Entity
{
    public const int TitleMinLength = 5;
    public const int TitleMaxLength = 100;
    public const int DescriptionMinLength = 10;
    public const int DescriptionMaxLength = 500;

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int VenueId { get; private set; }
    public Venue? Venue { get; private set; }
    public int MaxCapacity { get; private set; }

    /// <summary>Entradas aún disponibles. Se descuenta al reservar para tener control en tiempo real.</summary>
    public int AvailableTickets { get; private set; }

    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public decimal Price { get; private set; }
    public EventType Type { get; private set; }
    public EventStatus Status { get; private set; }

    private Event() { } // EF Core

    /// <summary>
    /// Fábrica que crea un evento válido o devuelve el error de negocio correspondiente.
    /// Valida invariantes propias del agregado (RN01 capacidad, RN03 horario nocturno y validaciones de RF-01).
    /// La no-superposición de venues (RN02) requiere consultar otros eventos y se valida en el handler.
    /// </summary>
    public static Result<Event> Create(
        string title,
        string description,
        Venue venue,
        int maxCapacity,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal price,
        EventType type,
        DateTimeOffset now)
    {
        title = title?.Trim() ?? string.Empty;
        description = description?.Trim() ?? string.Empty;

        if (title.Length is < TitleMinLength or > TitleMaxLength)
            return DomainErrors.Event.TitleInvalid;

        if (description.Length is < DescriptionMinLength or > DescriptionMaxLength)
            return DomainErrors.Event.DescriptionInvalid;

        if (maxCapacity <= 0)
            return DomainErrors.Event.CapacityNotPositive;

        if (price <= 0)
            return DomainErrors.Event.PriceNotPositive;

        if (startDate <= now)
            return DomainErrors.Event.StartMustBeFuture;

        if (endDate <= startDate)
            return DomainErrors.Event.EndMustBeAfterStart;

        // RN01: la capacidad del evento no puede exceder la del venue.
        if (maxCapacity > venue.Capacity)
            return DomainErrors.Event.ExceedsVenueCapacity;

        // RN03: en fin de semana no puede iniciar después de las 22:00.
        if (IsWeekendNight(startDate))
            return DomainErrors.Event.WeekendNightRestriction;

        Event @event = new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            VenueId = venue.Id,
            Venue = venue,
            MaxCapacity = maxCapacity,
            AvailableTickets = maxCapacity,
            StartDate = startDate,
            EndDate = endDate,
            Price = price,
            Type = type,
            Status = EventStatus.Activo
        };

        @event.RaiseDomainEvent(new EventCreatedDomainEvent(@event.Id, now));
        return @event;
    }

    /// <summary>Descuenta entradas de forma segura (RF-03: no exceder capacidad).</summary>
    public Result ReserveTickets(int quantity)
    {
        if (quantity < 1)
            return Result.Failure(DomainErrors.Reservation.InvalidQuantity);

        if (quantity > AvailableTickets)
            return Result.Failure(DomainErrors.Reservation.NotEnoughTickets);

        AvailableTickets -= quantity;
        return Result.Success();
    }

    /// <summary>Devuelve entradas al inventario (RF-05: liberar al cancelar) sin superar la capacidad.</summary>
    public void ReleaseTickets(int quantity)
    {
        AvailableTickets = Math.Min(MaxCapacity, AvailableTickets + quantity);
    }

    /// <summary>RN06: marca el evento como completado cuando la fecha actual supera su fin.</summary>
    public bool MarkAsCompletedIfFinished(DateTimeOffset now)
    {
        if (Status != EventStatus.Activo || now <= EndDate)
            return false;

        Status = EventStatus.Completado;
        RaiseDomainEvent(new EventCompletedDomainEvent(Id, now));
        return true;
    }

    public Result Cancel()
    {
        if (Status != EventStatus.Activo)
            return Result.Failure(Error.Conflict("Event.NotActive", "Solo se pueden cancelar eventos activos."));

        Status = EventStatus.Cancelado;
        return Result.Success();
    }

    /// <summary>Entradas confirmadas/comprometidas = capacidad - disponibles.</summary>
    public int SoldTickets => MaxCapacity - AvailableTickets;

    private static bool IsWeekendNight(DateTimeOffset start)
    {
        bool isWeekend = start.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        return isWeekend && start.TimeOfDay > new TimeSpan(22, 0, 0);
    }
}
