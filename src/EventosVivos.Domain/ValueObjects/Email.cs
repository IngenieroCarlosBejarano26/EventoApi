using System.Text.RegularExpressions;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Errors;

namespace EventosVivos.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    public const int MaxLength = 256;

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return DomainErrors.Email.Empty;

        email = email.Trim();

        if (email.Length > MaxLength)
            return DomainErrors.Email.TooLong;

        return !EmailRegex().IsMatch(email)
            ? DomainErrors.Email.Invalid
            : new Email(email);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
