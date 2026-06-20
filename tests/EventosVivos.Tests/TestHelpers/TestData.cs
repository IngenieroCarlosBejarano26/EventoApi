using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Tests.TestHelpers;

/// <summary>Reloj fijo para reglas temporales deterministas.</summary>
public sealed class FakeDateTimeProvider(DateTimeOffset now) : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; } = now;
}

public static class TestData
{
    // Lunes 5 de enero de 2026, 10:00 UTC.
    public static readonly DateTimeOffset Now = new(2026, 1, 5, 10, 0, 0, TimeSpan.Zero);

    public static Venue AuditorioCentral() => new(1, "Auditorio Central", 200, "Bogotá");
    public static Venue SalaNorte() => new(2, "Sala Norte", 50, "Bogotá");

    public static Event ValidEvent(
        Venue? venue = null,
        int maxCapacity = 100,
        decimal price = 50m,
        DateTimeOffset? start = null,
        DateTimeOffset? end = null,
        EventType type = EventType.Conferencia)
    {
        venue ??= AuditorioCentral();
        DateTimeOffset startDate = start ?? Now.AddDays(10);
        DateTimeOffset endDate = end ?? startDate.AddHours(2);

        Result<Event> result = Event.Create(
            "Evento de prueba",
            "Descripción válida del evento de prueba.",
            venue,
            maxCapacity,
            startDate,
            endDate,
            price,
            type,
            Now);

        if (result.IsFailure)
            throw new InvalidOperationException($"No se pudo crear el evento de prueba: {result.Error.Code}");

        return result.Value;
    }

    public static Email BuyerEmail() => Email.Create("comprador@test.com").Value;
}
