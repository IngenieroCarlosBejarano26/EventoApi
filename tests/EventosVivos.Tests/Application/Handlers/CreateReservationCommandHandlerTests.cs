using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Features.Reservations.CreateReservation;
using EventosVivos.Application.Features.Reservations.Shared;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Errors;
using EventosVivos.Tests.TestHelpers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EventosVivos.Tests.Application.Handlers;

public sealed class CreateReservationCommandHandlerTests
{
    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly IReservationRepository _reservations = Substitute.For<IReservationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly IDateTimeProvider _clock = new FakeDateTimeProvider(TestData.Now);

    private CreateReservationCommandHandler CreateSut() => new(_events, _reservations, _uow, _clock, _cache);

    [Fact]
    public async Task Handle_WhenEventNotFound_ShouldReturnNotFound()
    {
        _events.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns((Event?)null);

        CreateReservationCommand command = new(Guid.NewGuid(), 2, "Ana", "ana@test.com");
        Result<ReservationDto> result = await CreateSut().Handle(command, CancellationToken.None);

        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact] // Anti-sobreventa a nivel de aplicación
    public async Task Handle_WhenNotEnoughTickets_ShouldFail()
    {
        Event @event = TestData.ValidEvent(maxCapacity: 2);
        _events.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(@event);

        CreateReservationCommand command = new(@event.Id, 3, "Ana", "ana@test.com");
        Result<ReservationDto> result = await CreateSut().Handle(command, CancellationToken.None);

        result.Error.Should().Be(DomainErrors.Reservation.NotEnoughTickets);
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldPersistAndReturnDto()
    {
        Event @event = TestData.ValidEvent(maxCapacity: 100);
        _events.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(@event);

        CreateReservationCommand command = new(@event.Id, 3, "Ana", "ana@test.com");
        Result<ReservationDto> result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Quantity.Should().Be(3);
        _reservations.Received(1).Add(Arg.Any<Reservation>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldFail()
    {
        CreateReservationCommand command = new(Guid.NewGuid(), 1, "Ana", "no-es-email");

        Result<ReservationDto> result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
