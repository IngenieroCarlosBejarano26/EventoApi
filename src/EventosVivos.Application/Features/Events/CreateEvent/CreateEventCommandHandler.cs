using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Events.Shared;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Errors;

namespace EventosVivos.Application.Features.Events.CreateEvent;

internal sealed class CreateEventCommandHandler(
    IEventRepository eventRepository,
    IVenueRepository venueRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICacheService cache)
    : ICommandHandler<CreateEventCommand, EventDto>
{
    public async Task<Result<EventDto>> Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        Venue? venue = await venueRepository.GetByIdAsync(command.VenueId, cancellationToken);
        if (venue is null)
            return DomainErrors.Venue.NotFound(command.VenueId);

        // RN02: no permitir superposición de eventos activos en el mismo venue.
        bool hasOverlap = await eventRepository.HasOverlappingActiveEventAsync(
            command.VenueId, command.StartDate, command.EndDate, excludeEventId: null, cancellationToken);

        if (hasOverlap)
            return DomainErrors.Event.VenueOverlap;

        Result<Event> eventResult = Event.Create(
            command.Title,
            command.Description,
            venue,
            command.MaxCapacity,
            command.StartDate,
            command.EndDate,
            command.Price,
            command.Type,
            dateTimeProvider.UtcNow);

        if (eventResult.IsFailure)
            return Result.Failure<EventDto>(eventResult.Error);

        Event @event = eventResult.Value;
        eventRepository.Add(@event);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalida cachés afectadas por el alta.
        cache.RemoveByPrefix(CacheKeys.EventsPrefix);

        return @event.ToDto();
    }
}
