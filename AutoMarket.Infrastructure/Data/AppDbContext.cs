using AutoMarket.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<ListingImage> ListingImages { get; set; }
    public DbSet<ServiceRecord> ServiceRecords { get; set; }
    public DbSet<WatchlistItem> WatchlistItems { get; set; }
    public DbSet<Enquiry> Enquiries { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
