using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using FluentValidation;

namespace EventosVivos.Application.Features.Events.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Length(Event.TitleMinLength, Event.TitleMaxLength);

        RuleFor(x => x.Description)
            .NotEmpty()
            .Length(Event.DescriptionMinLength, Event.DescriptionMaxLength);

        RuleFor(x => x.VenueId)
            .GreaterThan(0);

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("La fecha de fin debe ser posterior al inicio.");

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}
