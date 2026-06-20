using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();
        builder.Property(v => v.Name).HasMaxLength(150).IsRequired();
        builder.Property(v => v.City).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Capacity).IsRequired();

        builder.Ignore(v => v.DomainEvents);

        builder.HasData(
            new { Id = 1, Name = "Auditorio Central", Capacity = 200, City = "Bogotá" },
            new { Id = 2, Name = "Sala Norte", Capacity = 50, City = "Bogotá" },
            new { Id = 3, Name = "Arena Sur", Capacity = 500, City = "Medellín" });
    }
}
