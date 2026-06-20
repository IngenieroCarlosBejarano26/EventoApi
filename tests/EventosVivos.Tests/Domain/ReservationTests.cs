using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Errors;
using EventosVivos.Domain.ValueObjects;
using EventosVivos.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Tests.Domain;

public sealed class ReservationTests
{
    private static readonly DateTimeOffset Now = TestData.Now;

    [Fact]
    public void Create_Valid_ShouldDecrementTicketsAndRaiseEvent()
    {
        Event @event = TestData.ValidEvent(maxCapacity: 100);

        Result<Reservation> result = Reservation.Create(@event, 3, "Ana", TestData.BuyerEmail(), Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ReservationStatus.PendientePago);
        @event.AvailableTickets.Should().Be(97);
        result.Value.DomainEvents.Should().ContainSingle();
    }

    [Fact] // Anti-sobreventa
    public void Create_WhenNotEnoughTickets_ShouldFail()
    {
        Event @event = TestData.ValidEvent(maxCapacity: 2);

        Result<Reservation> result = Reservation.Create(@event, 3, "Ana", TestData.BuyerEmail(), Now);

        result.Error.Should().Be(DomainErrors.Reservation.NotEnoughTickets);
        @event.AvailableTickets.Should().Be(2);
    }

    [Fact] // RN04 - menos de 1 hora
    public void Create_WhenLessThanOneHourToStart_ShouldFail()
    {
        Event @event = TestData.ValidEvent(start: Now.AddMinutes(30), end: Now.AddHours(2));

        Result<Reservation> result = Reservation.Create(@event, 1, "Ana", TestData.BuyerEmail(), Now);

        result.Error.Should().Be(DomainErrors.Reservation.TooLate);
    }

    [Fact] // RF-03 - menos de 24h, máximo 5
    public void Create_WhenWithin24HoursAndMoreThanFive_ShouldFail()
    {
        Event @event = TestData.ValidEvent(start: Now.AddHours(10), end: Now.AddHours(12));

        Result<Reservation> result = Reservation.Create(@event, 6, "Ana", TestData.BuyerEmail(), Now);

        result.Error.Should().Be(DomainErrors.Reservation.MaxFiveWithin24Hours);
    }

    [Fact] // RF-03 - menos de 24h, 5 permitido
    public void Create_WhenWithin24HoursAndExactlyFive_ShouldSucceed()
    {
        Event @event = TestData.ValidEvent(start: Now.AddHours(10), end: Now.AddHours(12));

        Result<Reservation> result = Reservation.Create(@event, 5, "Ana", TestData.BuyerEmail(), Now);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact] // RN05 - precio > 100, máximo 10
    public void Create_WhenPremiumPriceAndMoreThanTen_ShouldFail()
    {
        Event @event = TestData.ValidEvent(maxCapacity: 100, price: 150m);

        Result<Reservation> result = Reservation.Create(@event, 11, "Ana", TestData.BuyerEmail(), Now);

        result.Error.Should().Be(DomainErrors.Reservation.MaxTenForPremium);
    }

    [Fact] // RF-04
    public void Confirm_WhenPending_ShouldGenerateCodeAndConfirm()
    {
        Reservation reservation = CreatePendingReservation(out _);

        Result result = reservation.Confirm(ReservationCode.New(), Now);

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Confirmada);
        reservation.Code!.Value.Should().MatchRegex(@"^EV-\d{6}$");
    }

    [Fact] // RF-04 - ya confirmada
    public void Confirm_WhenAlreadyConfirmed_ShouldFail()
    {
        Reservation reservation = CreatePendingReservation(out _);
        reservation.Confirm(ReservationCode.New(), Now);

        Result result = reservation.Confirm(ReservationCode.New(), Now);

        result.Error.Should().Be(DomainErrors.Reservation.AlreadyConfirmed);
    }

    [Fact] // RF-05 - cancelar pendiente libera entradas
    public void Cancel_WhenPending_ShouldReleaseTickets()
    {
        Reservation reservation = CreatePendingReservation(out Event @event);
        int beforeAvailable = @event.AvailableTickets;

        Result result = reservation.Cancel(@event, Now);

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Cancelada);
        @event.AvailableTickets.Should().Be(beforeAvailable + reservation.Quantity);
    }

    [Fact] // RN07 - confirmada con < 48h => Perdida y NO libera
    public void Cancel_WhenConfirmedWithinPenaltyWindow_ShouldBeLostAndNotRelease()
    {
        Event @event = TestData.ValidEvent(maxCapacity: 100, start: Now.AddHours(24), end: Now.AddHours(26));
        Reservation reservation = Reservation.Create(@event, 4, "Ana", TestData.BuyerEmail(), Now).Value;
        reservation.Confirm(ReservationCode.New(), Now);
        int availableAfterReserve = @event.AvailableTickets;

        Result result = reservation.Cancel(@event, Now);

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Perdida);
        @event.AvailableTickets.Should().Be(availableAfterReserve); // no se liberó
    }

    [Fact] // Confirmada con > 48h => Cancelada y libera
    public void Cancel_WhenConfirmedOutsidePenaltyWindow_ShouldReleaseTickets()
    {
        Event @event = TestData.ValidEvent(maxCapacity: 100, start: Now.AddDays(10), end: Now.AddDays(10).AddHours(2));
        Reservation reservation = Reservation.Create(@event, 4, "Ana", TestData.BuyerEmail(), Now).Value;
        reservation.Confirm(ReservationCode.New(), Now);
        int availableAfterReserve = @event.AvailableTickets;

        Result result = reservation.Cancel(@event, Now);

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Cancelada);
        @event.AvailableTickets.Should().Be(availableAfterReserve + 4);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldFail()
    {
        Reservation reservation = CreatePendingReservation(out Event @event);
        reservation.Cancel(@event, Now);

        Result result = reservation.Cancel(@event, Now);

        result.Error.Should().Be(DomainErrors.Reservation.AlreadyCancelled);
    }

    private static Reservation CreatePendingReservation(out Event @event)
    {
        @event = TestData.ValidEvent(maxCapacity: 100);
        return Reservation.Create(@event, 2, "Ana", TestData.BuyerEmail(), Now).Value;
    }
}
