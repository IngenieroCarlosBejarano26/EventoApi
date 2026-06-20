using EventosVivos.Api.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

WebApplication app = builder.Build();

app.UseApiPipeline();

await app.InitializeDatabaseAsync();

await app.RunAsync();

public partial class Program;
