using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Reservations.Shared;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Errors;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Features.Reservations.ConfirmReservation;

internal sealed class ConfirmReservationCommandHandler(
    IReservationRepository reservationRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICacheService cache)
    : ICommandHandler<ConfirmReservationCommand, ReservationDto>
{
    public async Task<Result<ReservationDto>> Handle(ConfirmReservationCommand command, CancellationToken cancellationToken)
    {
        Reservation? reservation = await reservationRepository.GetByIdAsync(command.ReservationId, includeEvent: false, cancellationToken);
        if (reservation is null)
            return DomainErrors.Reservation.NotFound(command.ReservationId);

        Result confirmResult = reservation.Confirm(ReservationCode.New(), dateTimeProvider.UtcNow);
        if (confirmResult.IsFailure)
            return Result.Failure<ReservationDto>(confirmResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        cache.RemoveByPrefix(CacheKeys.ReportsPrefix);

        return reservation.ToDto();
    }
}
