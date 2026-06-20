using EventosVivos.Domain.Common;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Errors;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Entities;

public sealed class Reservation : Entity
{
    public const int LateBookingThresholdHours = 24;
    public const int LateBookingMaxTickets = 5;
    public const int PremiumPriceThreshold = 100;
    public const int PremiumMaxTickets = 10;
    public const int CancellationPenaltyHours = 48;

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Event? Event { get; private set; }
    public int Quantity { get; private set; }
    public string BuyerName { get; private set; } = string.Empty;
    public Email BuyerEmail { get; private set; } = null!;
    public ReservationStatus Status { get; private set; }
    public ReservationCode? Code { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    private Reservation() { } // EF Core

    public static Result<Reservation> Create(
        Event @event,
        int quantity,
        string buyerName,
        Email buyerEmail,
        DateTimeOffset now)
    {
        if (@event.Status != EventStatus.Activo)
            return DomainErrors.Reservation.EventNotActive;

        if (quantity < 1)
            return DomainErrors.Reservation.InvalidQuantity;

        TimeSpan timeUntilStart = @event.StartDate - now;

        if (timeUntilStart < TimeSpan.FromHours(1))
            return DomainErrors.Reservation.TooLate;

        if (timeUntilStart < TimeSpan.FromHours(LateBookingThresholdHours) && quantity > LateBookingMaxTickets)
            return DomainErrors.Reservation.MaxFiveWithin24Hours;

        if (@event.Price > PremiumPriceThreshold && quantity > PremiumMaxTickets)
            return DomainErrors.Reservation.MaxTenForPremium;

        Result reserveResult = @event.ReserveTickets(quantity);
        if (reserveResult.IsFailure)
            return Result.Failure<Reservation>(reserveResult.Error);

        Reservation reservation = new()
        {
            Id = Guid.NewGuid(),
            EventId = @event.Id,
            Quantity = quantity,
            BuyerName = buyerName.Trim(),
            BuyerEmail = buyerEmail,
            Status = ReservationStatus.PendientePago,
            CreatedAt = now
        };

        reservation.RaiseDomainEvent(new ReservationCreatedDomainEvent(
            reservation.Id, @event.Id, quantity, buyerEmail.Value, now));

        return reservation;
    }

    public Result Confirm(ReservationCode code, DateTimeOffset now)
    {
        if (Status == ReservationStatus.Confirmada)
            return Result.Failure(DomainErrors.Reservation.AlreadyConfirmed);

        if (Status is ReservationStatus.Cancelada or ReservationStatus.Perdida)
            return Result.Failure(DomainErrors.Reservation.CannotConfirmCancelled);

        Status = ReservationStatus.Confirmada;
        Code = code;
        ConfirmedAt = now;

        RaiseDomainEvent(new ReservationConfirmedDomainEvent(
            Id, EventId, code.Value, BuyerEmail.Value, now));

        return Result.Success();
    }

    public Result Cancel(Event @event, DateTimeOffset now)
    {
        if (Status is ReservationStatus.Cancelada or ReservationStatus.Perdida)
            return Result.Failure(DomainErrors.Reservation.AlreadyCancelled);

        bool wasConfirmed = Status == ReservationStatus.Confirmada;
        bool withinPenaltyWindow = (@event.StartDate - now) < TimeSpan.FromHours(CancellationPenaltyHours);

        bool ticketsReleased;
        if (wasConfirmed && withinPenaltyWindow)
        {
            Status = ReservationStatus.Perdida;
            ticketsReleased = false;
        }
        else
        {
            Status = ReservationStatus.Cancelada;
            @event.ReleaseTickets(Quantity);
            ticketsReleased = true;
        }

        CancelledAt = now;

        RaiseDomainEvent(new ReservationCancelledDomainEvent(
            Id, EventId, Quantity, ticketsReleased, Status, now));

        return Result.Success();
    }
}
