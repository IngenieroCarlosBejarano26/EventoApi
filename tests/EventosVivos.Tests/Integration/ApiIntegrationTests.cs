using System.Net;
using System.Net.Http.Json;
using EventosVivos.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventosVivos.Tests.Integration;

/// <summary>
/// Arranca la API real en memoria (TestServer) sustituyendo PostgreSQL por EF InMemory,
/// para validar los flujos de negocio de punta a punta a través de HTTP.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"eventos-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Elimina el proveedor PostgreSQL (DbContext + configuración de opciones acumulada)
            // antes de registrar EF InMemory; de lo contrario quedarían dos proveedores activos.
            List<ServiceDescriptor> toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                         || d.ServiceType == typeof(DbContextOptions)
                         || d.ServiceType == typeof(ApplicationDbContext)
                         || (d.ServiceType.FullName?.Contains("DbContextOptionsConfiguration") ?? false))
                .ToList();

            foreach (ServiceDescriptor descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_dbName));
        });
    }
}

public sealed class ApiIntegrationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string ApiKey = "eventosvivos-dev-api-key-2026";
    private static int _eventSequence;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetVenues_ShouldReturnSeededVenues()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/venues");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<VenueResponse>? venues = await response.Content.ReadFromJsonAsync<List<VenueResponse>>();
        venues.Should().NotBeNull();
        venues!.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateEvent_WithoutApiKey_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/events", NewEventPayload());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FullFlow_CreateEvent_Reserve_Confirm_ShouldReflectInOccupancyReport()
    {
        // RF-01: crear evento (operación protegida con API Key).
        EventResponse @event = await CreateEventAsync(maxCapacity: 10);
        @event.AvailableTickets.Should().Be(10);
        @event.Status.Should().Be("Activo");

        // RF-03: reservar 2 entradas -> queda pendiente de pago y descuenta inventario.
        HttpResponseMessage reserveResponse = await PostReservationAsync(@event.Id, quantity: 2);
        reserveResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        ReservationResponse? reservation = await reserveResponse.Content.ReadFromJsonAsync<ReservationResponse>();
        reservation!.Status.Should().Be("PendientePago");

        // RF-04: confirmar el pago -> genera código EV-xxxxxx.
        HttpRequestMessage confirm = new(HttpMethod.Post, $"/api/reservations/{reservation.Id}/confirm");
        confirm.Headers.Add("X-API-KEY", ApiKey);
        HttpResponseMessage confirmResponse = await _client.SendAsync(confirm);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ReservationResponse? confirmed = await confirmResponse.Content.ReadFromJsonAsync<ReservationResponse>();
        confirmed!.Status.Should().Be("Confirmada");
        confirmed.Code.Should().StartWith("EV-");

        // RF-06: el reporte de ocupación refleja las entradas vendidas (confirmadas).
        HttpResponseMessage reportResponse = await _client.GetAsync($"/api/events/{@event.Id}/occupancy-report");
        reportResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ReportResponse? report = await reportResponse.Content.ReadFromJsonAsync<ReportResponse>();
        report!.SoldTickets.Should().Be(2);
        report.AvailableTickets.Should().Be(8);
    }

    [Fact]
    public async Task Reserve_BeyondCapacity_ShouldFail() // Anti-sobreventa end-to-end
    {
        EventResponse @event = await CreateEventAsync(maxCapacity: 2);

        HttpResponseMessage response = await PostReservationAsync(@event.Id, quantity: 3);

        response.IsSuccessStatusCode.Should().BeFalse();
        ((int)response.StatusCode).Should().BeOneOf(400, 409);
    }

    [Fact]
    public async Task GetEvents_ShouldContainCreatedEvent()
    {
        EventResponse created = await CreateEventAsync(maxCapacity: 5);

        HttpResponseMessage response = await _client.GetAsync("/api/events");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<EventResponse>? events = await response.Content.ReadFromJsonAsync<List<EventResponse>>();
        events!.Select(e => e.Id).Should().Contain(created.Id);
    }

    // ---- Helpers ----

    private async Task<EventResponse> CreateEventAsync(int maxCapacity)
    {
        HttpRequestMessage request = new(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(NewEventPayload(maxCapacity)),
        };
        request.Headers.Add("X-API-KEY", ApiKey);

        HttpResponseMessage response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<EventResponse>())!;
    }

    private async Task<HttpResponseMessage> PostReservationAsync(Guid eventId, int quantity)
    {
        HttpRequestMessage request = new(HttpMethod.Post, "/api/reservations")
        {
            Content = JsonContent.Create(new
            {
                eventId,
                quantity,
                buyerName = "Comprador de Prueba",
                buyerEmail = "comprador@test.com",
            }),
        };
        request.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
        return await _client.SendAsync(request);
    }

    private static object NewEventPayload(int maxCapacity = 10)
    {
        // Cada evento usa un día distinto para no solapar en el mismo venue (RN02).
        int offset = Interlocked.Increment(ref _eventSequence);
        DateTimeOffset start = DateTimeOffset.UtcNow.Date.AddDays(15 + offset).AddHours(12);
        return new
        {
            title = "Evento de Integración",
            description = "Evento creado por las pruebas de integración end to end.",
            venueId = 1, // Auditorio Central (capacidad 200)
            maxCapacity,
            startDate = start,
            endDate = start.AddHours(2),
            price = 50m,
            type = "Conferencia",
        };
    }

    private sealed record VenueResponse(int Id, string Name, int Capacity, string City);

    private sealed record EventResponse(Guid Id, int MaxCapacity, int AvailableTickets, string Status, string Type);

    private sealed record ReservationResponse(Guid Id, int Quantity, string Status, string? Code);

    private sealed record ReportResponse(int SoldTickets, int AvailableTickets, decimal OccupancyPercentage);
}
