namespace EventosVivos.Application.Common.Abstractions;

/// <summary>Claves y prefijos de cache centralizados para evitar strings mágicos dispersos.</summary>
public static class CacheKeys
{
    public const string EventsPrefix = "events:";
    public const string VenuesPrefix = "venues:";
    public const string ReportsPrefix = "reports:";

    public static string EventsList(string filterHash) => $"{EventsPrefix}list:{filterHash}";
    public static string VenuesAll => $"{VenuesPrefix}all";
    public static string OccupancyReport(Guid eventId) => $"{ReportsPrefix}occupancy:{eventId}";
}
