using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Errors;
using EventosVivos.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Tests.Domain;

public sealed class EventTests
{
    private static readonly DateTimeOffset Now = TestData.Now;

    [Fact]
    public void Create_WithValidData_ShouldSucceedAndSetAvailableTicketsToCapacity()
    {
        Result<Event> result = Event.Create(
            "Conferencia .NET", "Una gran conferencia sobre .NET 10.",
            TestData.AuditorioCentral(), 150, Now.AddDays(5), Now.AddDays(5).AddHours(3),
            80m, EventType.Conferencia, Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.AvailableTickets.Should().Be(150);
        result.Value.Status.Should().Be(EventStatus.Activo);
    }

    [Fact] // RN01
    public void Create_WhenCapacityExceedsVenue_ShouldFail()
    {
        Result<Event> result = Event.Create(
            "Evento grande", "Descripción válida del evento.",
            TestData.SalaNorte(), 100, Now.AddDays(5), Now.AddDays(5).AddHours(2),
            50m, EventType.Concierto, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Event.ExceedsVenueCapacity);
    }

    [Fact] // RN03 - sábado 23:00 hora Bogotá
    public void Create_OnWeekendAfter22_ShouldFail()
    {
        DateTimeOffset saturdayNightBogota = new(2026, 1, 11, 4, 0, 0, TimeSpan.Zero);

        Result<Event> result = Event.Create(
            "Concierto nocturno", "Descripción válida del evento.",
            TestData.AuditorioCentral(), 100, saturdayNightBogota, saturdayNightBogota.AddHours(2),
            50m, EventType.Concierto, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Event.WeekendNightRestriction);
    }

    [Fact] // RN03 - sábado 22:00 exacto en Bogotá debe permitirse
    public void Create_AtSaturday22InBusinessTimeZone_ShouldSucceed()
    {
        DateTimeOffset saturday22Bogota = new(2026, 1, 11, 3, 0, 0, TimeSpan.Zero);

        Result<Event> result = Event.Create(
            "Concierto temprano", "Descripción válida del evento.",
            TestData.AuditorioCentral(), 100, saturday22Bogota, saturday22Bogota.AddHours(2),
            50m, EventType.Concierto, Now);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact] // RN03 - domingo en UTC pero sábado 23:00 en Bogotá
    public void Create_WhenUtcIsSundayButBusinessTimeIsSaturdayNight_ShouldFail()
    {
        DateTimeOffset sundayMorningUtc = new(2026, 1, 11, 4, 30, 0, TimeSpan.Zero);

        Result<Event> result = Event.Create(
            "Concierto nocturno", "Descripción válida del evento.",
            TestData.AuditorioCentral(), 100, sundayMorningUtc, sundayMorningUtc.AddHours(2),
            50m, EventType.Concierto, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Event.WeekendNightRestriction);
    }

    [Fact]
    public void Create_WithPastStartDate_ShouldFail()
    {
        Result<Event> result = Event.Create(
            "Evento pasado", "Descripción válida del evento.",
            TestData.AuditorioCentral(), 100, Now.AddDays(-1), Now.AddDays(-1).AddHours(2),
            50m, EventType.Taller, Now);

        result.Error.Should().Be(DomainErrors.Event.StartMustBeFuture);
    }

    [Theory]
    [InlineData("abc")] // < 5
    public void Create_WithInvalidTitle_ShouldFail(string title)
    {
        Result<Event> result = Event.Create(
            title, "Descripción válida del evento.",
            TestData.AuditorioCentral(), 100, Now.AddDays(5), Now.AddDays(5).AddHours(2),
            50m, EventType.Taller, Now);

        result.Error.Should().Be(DomainErrors.Event.TitleInvalid);
    }

    [Fact] // Anti-sobreventa a nivel de entidad
    public void ReserveTickets_BeyondAvailability_ShouldFail()
    {
        Event @event = TestData.ValidEvent(maxCapacity: 10);

        @event.ReserveTickets(8).IsSuccess.Should().BeTrue();
        @event.AvailableTickets.Should().Be(2);

        Result result = @event.ReserveTickets(3);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Reservation.NotEnoughTickets);
        @event.AvailableTickets.Should().Be(2);
    }

    [Fact]
    public void ReleaseTickets_ShouldNotExceedMaxCapacity()
    {
        Event @event = TestData.ValidEvent(maxCapacity: 10);
        @event.ReserveTickets(5);

        @event.ReleaseTickets(20);

        @event.AvailableTickets.Should().Be(10);
    }

    [Fact] // RN06
    public void MarkAsCompletedIfFinished_WhenEnded_ShouldComplete()
    {
        Event @event = TestData.ValidEvent(start: Now.AddHours(2), end: Now.AddHours(4));

        bool changed = @event.MarkAsCompletedIfFinished(Now.AddHours(5));

        changed.Should().BeTrue();
        @event.Status.Should().Be(EventStatus.Completado);
    }

    [Fact] // RN06 - no completa si aún no termina
    public void MarkAsCompletedIfFinished_WhenNotEnded_ShouldNotChange()
    {
        Event @event = TestData.ValidEvent(start: Now.AddDays(1), end: Now.AddDays(1).AddHours(2));

        bool changed = @event.MarkAsCompletedIfFinished(Now);

        changed.Should().BeFalse();
        @event.Status.Should().Be(EventStatus.Activo);
    }
}
