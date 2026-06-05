using AutoMarket.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoMarket.Infrastructure.Data.Configurations;

public class EnquiryConfiguration : IEntityTypeConfiguration<Enquiry>
{
    public void Configure(EntityTypeBuilder<Enquiry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.SellerReply)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Two FK to User — one must use Restrict to avoid multiple cascade paths
        builder.HasOne(e => e.Buyer)
            .WithMany(u => u.SentEnquiries)
            .HasForeignKey(e => e.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Seller)
            .WithMany(u => u.ReceivedEnquiries)
            .HasForeignKey(e => e.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Listing)
            .WithMany(l => l.Enquiries)
            .HasForeignKey(e => e.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
