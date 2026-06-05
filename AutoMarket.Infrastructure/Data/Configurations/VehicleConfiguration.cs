using AutoMarket.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoMarket.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.VIN)
            .IsRequired()
            .HasMaxLength(17);

        builder.Property(v => v.Make)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.EngineType)
            .HasMaxLength(100);

        builder.Property(v => v.FuelType)
            .HasMaxLength(50);

        builder.Property(v => v.Color)
            .HasMaxLength(50);

        builder.HasIndex(v => v.VIN).IsUnique();
    }
}
