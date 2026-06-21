using EventosVivos.Domain.Common;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Errors;
using EventosVivos.Domain.Events;

namespace EventosVivos.Domain.Entities;

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

    public int AvailableTickets { get; private set; }

    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public decimal Price { get; private set; }
    public EventType Type { get; private set; }
    public EventStatus Status { get; private set; }

    private Event() { } // EF Core

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

        if (maxCapacity > venue.Capacity)
            return DomainErrors.Event.ExceedsVenueCapacity;

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

    public Result ReserveTickets(int quantity)
    {
        if (quantity < 1)
            return Result.Failure(DomainErrors.Reservation.InvalidQuantity);

        if (quantity > AvailableTickets)
            return Result.Failure(DomainErrors.Reservation.NotEnoughTickets);

        AvailableTickets -= quantity;
        return Result.Success();
    }

    public void ReleaseTickets(int quantity)
    {
        AvailableTickets = Math.Min(MaxCapacity, AvailableTickets + quantity);
    }

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

    public int SoldTickets => MaxCapacity - AvailableTickets;

    private static bool IsWeekendNight(DateTimeOffset start)
    {
        DateTimeOffset localStart = BusinessTimeZone.ToLocal(start);
        bool isWeekend = localStart.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        return isWeekend && localStart.TimeOfDay > new TimeSpan(22, 0, 0);
    }
}
