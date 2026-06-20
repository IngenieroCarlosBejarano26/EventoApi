using FluentValidation;

namespace EventosVivos.Application.Features.Reservations.ConfirmReservation;

public sealed class ConfirmReservationCommandValidator : AbstractValidator<ConfirmReservationCommand>
{
    public ConfirmReservationCommandValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
    }
}
