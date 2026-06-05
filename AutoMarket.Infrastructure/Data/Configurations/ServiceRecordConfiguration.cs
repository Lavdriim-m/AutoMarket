using AutoMarket.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoMarket.Infrastructure.Data.Configurations;

public class ServiceRecordConfiguration : IEntityTypeConfiguration<ServiceRecord>
{
    public void Configure(EntityTypeBuilder<ServiceRecord> builder)
    {
        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.ServiceType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(sr => sr.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(sr => sr.Cost)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(sr => sr.Vehicle)
            .WithMany(v => v.ServiceRecords)
            .HasForeignKey(sr => sr.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sr => sr.Mechanic)
            .WithMany(u => u.ServiceRecords)
            .HasForeignKey(sr => sr.MechanicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
