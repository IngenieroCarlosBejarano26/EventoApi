using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Features.Events.CreateEvent;
using EventosVivos.Application.Features.Events.Shared;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Errors;
using EventosVivos.Tests.TestHelpers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EventosVivos.Tests.Application.Handlers;

public sealed class CreateEventCommandHandlerTests
{
    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly IVenueRepository _venues = Substitute.For<IVenueRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly IDateTimeProvider _clock = new FakeDateTimeProvider(TestData.Now);
    private readonly IEventCompletionService _eventCompletion = Substitute.For<IEventCompletionService>();

    private CreateEventCommandHandler CreateSut() =>
        new(_events, _venues, _uow, _clock, _eventCompletion, _cache);

    private static CreateEventCommand ValidCommand(int venueId = 1, int capacity = 100) =>
        new("Conferencia .NET", "Descripción válida del evento.",
            venueId, capacity, TestData.Now.AddDays(5), TestData.Now.AddDays(5).AddHours(2), 50m, EventType.Conferencia);

    [Fact]
    public async Task Handle_WhenVenueNotFound_ShouldReturnNotFound()
    {
        _venues.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Venue?)null);

        Result<EventDto> result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact] // RN02
    public async Task Handle_WhenVenueOverlap_ShouldReturnConflict()
    {
        _venues.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(TestData.AuditorioCentral());
        _events.HasOverlappingActiveEventAsync(
            Arg.Any<int>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
            Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(true);

        Result<EventDto> result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        result.Error.Should().Be(DomainErrors.Event.VenueOverlap);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldPersistAndReturnDto()
    {
        _venues.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(TestData.AuditorioCentral());
        _events.HasOverlappingActiveEventAsync(
            Arg.Any<int>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
            Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);

        Result<EventDto> result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Conferencia .NET");
        _events.Received(1).Add(Arg.Any<Event>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _cache.Received().RemoveByPrefix(CacheKeys.EventsPrefix);
    }
}
