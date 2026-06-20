using FluentValidation;

namespace EventosVivos.Application.Features.Reservations.CancelReservation;

public sealed class CancelReservationCommandValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationCommandValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
    }
}
