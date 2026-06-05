using AutoMarket.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoMarket.Infrastructure.Data.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(l => l.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(l => l.Seller)
            .WithMany(u => u.Listings)
            .HasForeignKey(l => l.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Vehicle)
            .WithOne(v => v.Listing)
            .HasForeignKey<Listing>(l => l.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Images)
            .WithOne(i => i.Listing)
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
