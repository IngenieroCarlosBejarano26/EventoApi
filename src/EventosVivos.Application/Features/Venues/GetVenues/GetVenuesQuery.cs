using EventosVivos.Application.Common.Messaging;

namespace EventosVivos.Application.Features.Venues.GetVenues;

public sealed record VenueDto(int Id, string Name, int Capacity, string City);

public sealed record GetVenuesQuery : IQuery<IReadOnlyList<VenueDto>>;
