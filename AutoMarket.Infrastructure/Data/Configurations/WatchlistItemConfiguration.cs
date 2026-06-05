using AutoMarket.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoMarket.Infrastructure.Data.Configurations;

public class WatchlistItemConfiguration : IEntityTypeConfiguration<WatchlistItem>
{
    public void Configure(EntityTypeBuilder<WatchlistItem> builder)
    {
        builder.HasKey(wi => wi.Id);

        builder.HasIndex(wi => new { wi.BuyerId, wi.ListingId }).IsUnique();

        builder.HasOne(wi => wi.Buyer)
            .WithMany(u => u.WatchlistItems)
            .HasForeignKey(wi => wi.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wi => wi.Listing)
            .WithMany(l => l.WatchlistItems)
            .HasForeignKey(wi => wi.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
