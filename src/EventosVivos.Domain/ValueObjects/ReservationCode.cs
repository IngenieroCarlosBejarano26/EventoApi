using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.ValueObjects;

public sealed class ReservationCode : ValueObject
{
    public string Value { get; }

    private ReservationCode(string value) => Value = value;

    public static ReservationCode New() =>
        new($"EV-{Random.Shared.Next(0, 1_000_000):D6}");

    public static ReservationCode FromValue(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
