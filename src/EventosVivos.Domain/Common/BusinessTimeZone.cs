namespace EventosVivos.Domain.Common;

public static class BusinessTimeZone
{
    public const string IanaId = "America/Bogota";

    public static TimeZoneInfo Zone { get; } = TimeZoneInfo.FindSystemTimeZoneById(IanaId);

    public static DateTimeOffset ToLocal(DateTimeOffset instant) =>
        TimeZoneInfo.ConvertTime(instant, Zone);
}
