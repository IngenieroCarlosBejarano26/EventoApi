using FluentValidation;

namespace EventosVivos.Application.Features.Reservations.CreateReservation;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.BuyerName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.BuyerEmail)
            .NotEmpty()
            .EmailAddress();
    }
}
