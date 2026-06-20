using EventosVivos.Application.Features.Events.CreateEvent;
using EventosVivos.Application.Features.Reservations.CreateReservation;
using EventosVivos.Domain.Enums;
using EventosVivos.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Tests.Application.Validators;

public sealed class ValidatorTests
{
    private readonly CreateEventCommandValidator _eventValidator = new();
    private readonly CreateReservationCommandValidator _reservationValidator = new();

    [Fact]
    public void CreateEvent_WithValidCommand_ShouldBeValid()
    {
        CreateEventCommand command = new(
            "Conferencia válida", "Descripción suficientemente larga.",
            1, 100, TestData.Now.AddDays(5), TestData.Now.AddDays(5).AddHours(2), 50m, EventType.Conferencia);

        _eventValidator.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateEvent_WithShortTitle_ShouldBeInvalid()
    {
        CreateEventCommand command = new(
            "abc", "Descripción suficientemente larga.",
            1, 100, TestData.Now.AddDays(5), TestData.Now.AddDays(5).AddHours(2), 50m, EventType.Conferencia);

        _eventValidator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateEvent_WithEndBeforeStart_ShouldBeInvalid()
    {
        CreateEventCommand command = new(
            "Conferencia válida", "Descripción suficientemente larga.",
            1, 100, TestData.Now.AddDays(5), TestData.Now.AddDays(4), 50m, EventType.Conferencia);

        _eventValidator.Validate(command).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateReservation_WithInvalidQuantity_ShouldBeInvalid(int quantity)
    {
        CreateReservationCommand command = new(Guid.NewGuid(), quantity, "Ana", "ana@test.com");

        _reservationValidator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateReservation_WithInvalidEmail_ShouldBeInvalid()
    {
        CreateReservationCommand command = new(Guid.NewGuid(), 1, "Ana", "no-es-email");

        _reservationValidator.Validate(command).IsValid.Should().BeFalse();
    }
}
