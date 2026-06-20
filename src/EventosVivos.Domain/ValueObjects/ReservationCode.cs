using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.ValueObjects;

/// <summary>
/// Value Object ReservationCode con formato EV-{6 dígitos} (RF-04).
/// La unicidad efectiva se garantiza con un índice único en base de datos.
/// </summary>
public sealed class ReservationCode : ValueObject
{
    public string Value { get; }

    private ReservationCode(string value) => Value = value;

    /// <summary>Genera un nuevo código aleatorio en formato EV-000000.</summary>
    public static ReservationCode New() =>
        new($"EV-{Random.Shared.Next(0, 1_000_000):D6}");

    /// <summary>Rehidrata un código existente (uso de persistencia).</summary>
    public static ReservationCode FromValue(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
