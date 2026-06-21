using EventosVivos.Application.Common.Messaging;

namespace EventosVivos.Application.Features.Venues.GetVenues;

public sealed record GetVenuesQuery : IQuery<IReadOnlyList<VenueDto>>;
