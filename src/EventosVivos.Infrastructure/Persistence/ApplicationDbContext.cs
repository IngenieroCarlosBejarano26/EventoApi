using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Recolecta y limpia los eventos antes de persistir.
        List<IDomainEvent> domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return events;
            })
            .ToList();

        try
        {
            int result = await base.SaveChangesAsync(cancellationToken);

            // Publica tras confirmar la persistencia (los handlers actuales son de side-effects).
            foreach (IDomainEvent domainEvent in domainEvents)
                await publisher.Publish(domainEvent, cancellationToken);

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(
                "El registro fue modificado por otra operación concurrente.", ex);
        }
    }
}
