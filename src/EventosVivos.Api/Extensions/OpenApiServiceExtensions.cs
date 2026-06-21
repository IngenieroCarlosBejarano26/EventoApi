using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EventosVivos.Api.Extensions;

public static class OpenApiServiceExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services) =>
        services
            .AddOpenApi(options =>
            {
                options.AddDocumentTransformer<ApiDocumentTransformer>();
                options.AddOperationTransformer<IdempotencyOperationTransformer>();
            });
}
