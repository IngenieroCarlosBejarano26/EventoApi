using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Features.Reservations.CancelReservation;
using EventosVivos.Application.Features.Reservations.ConfirmReservation;
using EventosVivos.Application.Features.Reservations.Shared;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Errors;
using EventosVivos.Domain.ValueObjects;
using EventosVivos.Tests.TestHelpers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EventosVivos.Tests.Application.Handlers;

public sealed class ConfirmAndCancelReservationHandlerTests
{
    private readonly IReservationRepository _reservations = Substitute.For<IReservationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly IDateTimeProvider _clock = new FakeDateTimeProvider(TestData.Now);
    private readonly IEventCompletionService _eventCompletion = Substitute.For<IEventCompletionService>();

    [Fact] // RF-04
    public async Task Confirm_WhenPending_ShouldConfirmAndGenerateCode()
    {
        Reservation reservation = NewPendingReservation(out _);
        _reservations.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(reservation);

        ConfirmReservationCommandHandler sut = new(_reservations, _uow, _clock, _eventCompletion, _cache);
        Result<ReservationDto> result = await sut.Handle(new ConfirmReservationCommand(reservation.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ReservationStatus.Confirmada);
        result.Value.Code.Should().MatchRegex(@"^EV-\d{6}$");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // RF-04 - ya confirmada
    public async Task Confirm_WhenAlreadyConfirmed_ShouldFail()
    {
        Reservation reservation = NewPendingReservation(out _);
        reservation.Confirm(ReservationCode.New(), TestData.Now);
        _reservations.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(reservation);

        ConfirmReservationCommandHandler sut = new(_reservations, _uow, _clock, _eventCompletion, _cache);
        Result<ReservationDto> result = await sut.Handle(new ConfirmReservationCommand(reservation.Id), CancellationToken.None);

        result.Error.Should().Be(DomainErrors.Reservation.AlreadyConfirmed);
    }

    [Fact] // RF-05
    public async Task Cancel_WhenPending_ShouldCancelAndReleaseTickets()
    {
        Reservation reservation = NewPendingReservation(out Event @event);
        _reservations.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(reservation);

        CancelReservationCommandHandler sut = new(_reservations, _uow, _clock, _eventCompletion, _cache);
        Result<ReservationDto> result = await sut.Handle(new CancelReservationCommand(reservation.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ReservationStatus.Cancelada);
        @event.AvailableTickets.Should().Be(@event.MaxCapacity);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static Reservation NewPendingReservation(out Event @event)
    {
        @event = TestData.ValidEvent(maxCapacity: 100);
        Reservation reservation = Reservation.Create(@event, 2, "Ana", TestData.BuyerEmail(), TestData.Now).Value;
        typeof(Reservation).GetProperty(nameof(Reservation.Event))!.SetValue(reservation, @event);
        return reservation;
    }
}
