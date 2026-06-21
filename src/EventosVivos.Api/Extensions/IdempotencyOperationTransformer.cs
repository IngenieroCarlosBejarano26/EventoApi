using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EventosVivos.Api.Extensions;

internal sealed class IdempotencyOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor.RouteValues.TryGetValue("action", out string? action)
            && action == "Create"
            && context.Description.RelativePath?.Contains("reservations", StringComparison.OrdinalIgnoreCase) == true)
        {
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Idempotency-Key",
                In = ParameterLocation.Header,
                Required = false,
                Description = "UUID recomendado. Garantiza idempotencia en la creación de reservas.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" }
            });
        }

        return Task.CompletedTask;
    }
}
