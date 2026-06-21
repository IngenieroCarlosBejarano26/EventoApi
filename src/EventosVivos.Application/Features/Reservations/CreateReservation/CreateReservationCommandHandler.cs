using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Reservations.Shared;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Errors;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Features.Reservations.CreateReservation;

internal sealed class CreateReservationCommandHandler(
    IEventRepository eventRepository,
    IReservationRepository reservationRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IEventCompletionService eventCompletionService,
    ICacheService cache)
    : ICommandHandler<CreateReservationCommand, ReservationDto>
{
    private const int MaxConcurrencyRetries = 3;

    public async Task<Result<ReservationDto>> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        Result<Email> emailResult = Email.Create(command.BuyerEmail);
        if (emailResult.IsFailure)
            return Result.Failure<ReservationDto>(emailResult.Error);

        await eventCompletionService.CompleteFinishedEventsAsync(cancellationToken);

        for (int attempt = 1; ; attempt++)
        {
            Event? @event = await eventRepository.GetByIdAsync(command.EventId, includeVenue: false, cancellationToken);
            if (@event is null)
                return DomainErrors.Event.NotFound(command.EventId);

            Result<Reservation> reservationResult = Reservation.Create(
                @event, command.Quantity, command.BuyerName, emailResult.Value, dateTimeProvider.UtcNow);

            if (reservationResult.IsFailure)
                return Result.Failure<ReservationDto>(reservationResult.Error);

            Reservation reservation = reservationResult.Value;
            reservationRepository.Add(reservation);

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (ConcurrencyConflictException) when (attempt < MaxConcurrencyRetries)
            {
                continue; // Otro proceso modificÃ³ el evento; recargamos y reintentamos.
            }
            catch (ConcurrencyConflictException)
            {
                return Result.Failure<ReservationDto>(DomainErrors.Reservation.NotEnoughTickets);
            }

            cache.RemoveByPrefix(CacheKeys.ReportsPrefix);
            cache.RemoveByPrefix(CacheKeys.EventsPrefix);

            return reservation.ToDto();
        }
    }
}
