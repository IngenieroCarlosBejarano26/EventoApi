using EventosVivos.Application.Common.Abstractions;

namespace EventosVivos.Infrastructure.Services;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
