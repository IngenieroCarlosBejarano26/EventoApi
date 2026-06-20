namespace EventosVivos.Domain.Common;

/// <summary>
/// Base para Value Objects: igualdad estructural por componentes e inmutabilidad.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other) => other is not null && ValuesAreEqual(other);

    public override bool Equals(object? obj) => obj is ValueObject other && ValuesAreEqual(other);

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(default(int), (hash, value) => HashCode.Combine(hash, value));

    private bool ValuesAreEqual(ValueObject other) =>
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
