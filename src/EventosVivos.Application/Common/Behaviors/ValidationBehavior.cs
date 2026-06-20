using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ValidationException = EventosVivos.Application.Common.Exceptions.ValidationException;

namespace EventosVivos.Application.Common.Behaviors;

/// <summary>
/// Behavior de MediatR que ejecuta automáticamente todos los validadores FluentValidation
/// registrados para el request antes de llegar al handler.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<ValidationFailure> failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            Dictionary<string, string[]> errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

            throw new ValidationException(errors);
        }

        return await next();
    }
}
