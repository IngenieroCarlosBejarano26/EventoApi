using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Reservations.Shared;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Errors;

namespace EventosVivos.Application.Features.Reservations.CancelReservation;

internal sealed class CancelReservationCommandHandler(
    IReservationRepository reservationRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IEventCompletionService eventCompletionService,
    ICacheService cache)
    : ICommandHandler<CancelReservationCommand, ReservationDto>
{
    public async Task<Result<ReservationDto>> Handle(CancelReservationCommand command, CancellationToken cancellationToken)
    {
        await eventCompletionService.CompleteFinishedEventsAsync(cancellationToken);

        Reservation? reservation = await reservationRepository.GetByIdAsync(command.ReservationId, includeEvent: true, cancellationToken);
        if (reservation is null)
            return DomainErrors.Reservation.NotFound(command.ReservationId);

        if (reservation.Event is null)
            return DomainErrors.Event.NotFound(reservation.EventId);

        Result cancelResult = reservation.Cancel(reservation.Event, dateTimeProvider.UtcNow);
        if (cancelResult.IsFailure)
            return Result.Failure<ReservationDto>(cancelResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        cache.RemoveByPrefix(CacheKeys.ReportsPrefix);
        cache.RemoveByPrefix(CacheKeys.EventsPrefix);

        return reservation.ToDto();
    }
}
