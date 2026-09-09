using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.API.Models;

namespace ReservationSystem.API.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Resource> Resources { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure enum storage as strings
        modelBuilder.Entity<Resource>()
            .Property(r => r.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion<string>();

        // Configure relationships
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Resource)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.ResourceId);

        // Seed data for testing
        modelBuilder.Entity<Resource>().HasData(
            new Resource 
            { 
                Id = 1, 
                Name = "Conference Room A", 
                Type = ReservationSystem.API.Enums.ResourceType.Room, 
                Capacity = 10, 
                IsActive = true 
            }
        );
    }
}
