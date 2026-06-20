using EventosVivos.Domain.Entities;
using EventosVivos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Quantity).IsRequired();
        builder.Property(r => r.BuyerName).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.ConfirmedAt);
        builder.Property(r => r.CancelledAt);

        // Value Object Email -> string.
        builder.Property(r => r.BuyerEmail)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .HasColumnName("buyer_email")
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        // Value Object ReservationCode (nullable) -> string, único cuando existe.
        builder.Property(r => r.Code)
            .HasConversion(code => code!.Value, value => ReservationCode.FromValue(value))
            .HasColumnName("code")
            .HasMaxLength(20);

        builder.HasIndex(r => r.Code).IsUnique();
        builder.HasIndex(r => r.EventId);

        builder.HasOne(r => r.Event)
            .WithMany()
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Concurrencia optimista mapeada a la columna de sistema "xmin" de PostgreSQL.
        builder.Property<uint>("Version")
            .IsRowVersion();

        builder.Ignore(r => r.DomainEvents);
    }
}
