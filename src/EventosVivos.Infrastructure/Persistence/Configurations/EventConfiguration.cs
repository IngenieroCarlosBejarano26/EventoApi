using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).HasMaxLength(Event.TitleMaxLength).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(Event.DescriptionMaxLength).IsRequired();
        builder.Property(e => e.MaxCapacity).IsRequired();
        builder.Property(e => e.AvailableTickets).IsRequired();
        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.EndDate).IsRequired();
        builder.Property(e => e.Price).HasColumnType("numeric(12,2)").IsRequired();

        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(e => e.Venue)
            .WithMany()
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.VenueId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.StartDate);

        // Concurrencia optimista mapeada a la columna de sistema "xmin" de PostgreSQL (evita sobreventa).
        builder.Property<uint>("Version")
            .IsRowVersion();

        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.SoldTickets);
    }
}
