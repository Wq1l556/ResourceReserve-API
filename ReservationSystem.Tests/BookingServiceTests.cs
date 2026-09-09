using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.API.Data;
using ReservationSystem.API.DTOs;
using ReservationSystem.API.Enums;
using ReservationSystem.API.Models;
using ReservationSystem.API.Services;
using Xunit;

namespace ReservationSystem.Tests;

public class BookingServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        // Seed default resource
        context.Resources.Add(new Resource
        {
            Id = 1,
            Name = "Boardroom A",
            Type = ResourceType.Room,
            Capacity = 12,
            IsActive = true
        });
        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task IsResourceAvailableAsync_ExactMatch_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new BookingService(context);
        var baseStart = new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc);
        var baseEnd = new DateTime(2026, 9, 15, 11, 0, 0, DateTimeKind.Utc);

        context.Bookings.Add(new Booking
        {
            Id = 1,
            ResourceId = 1,
            UserId = "user1",
            StartTime = baseStart,
            EndTime = baseEnd,
            Status = BookingStatus.Approved,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act - Request exact same time slot
        bool available = await service.IsResourceAvailableAsync(1, baseStart, baseEnd);

        // Assert
        Assert.False(available);
    }

    [Fact]
    public async Task IsResourceAvailableAsync_PartialOverlap_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new BookingService(context);
        var baseStart = new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc);
        var baseEnd = new DateTime(2026, 9, 15, 11, 0, 0, DateTimeKind.Utc);

        context.Bookings.Add(new Booking
        {
            Id = 1,
            ResourceId = 1,
            UserId = "user1",
            StartTime = baseStart,
            EndTime = baseEnd,
            Status = BookingStatus.Approved,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act - Overlapping partially inside the slot: 10:15 - 10:45
        bool availableInside = await service.IsResourceAvailableAsync(
            1,
            new DateTime(2026, 9, 15, 10, 15, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 15, 10, 45, 0, DateTimeKind.Utc));

        // Act - Overlapping start: 09:30 - 10:30
        bool availableStartOverlap = await service.IsResourceAvailableAsync(
            1,
            new DateTime(2026, 9, 15, 9, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 15, 10, 30, 0, DateTimeKind.Utc));

        // Act - Overlapping end: 10:30 - 11:30
        bool availableEndOverlap = await service.IsResourceAvailableAsync(
            1,
            new DateTime(2026, 9, 15, 10, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 15, 11, 30, 0, DateTimeKind.Utc));

        // Assert
        Assert.False(availableInside);
        Assert.False(availableStartOverlap);
        Assert.False(availableEndOverlap);
    }

    [Fact]
    public async Task IsResourceAvailableAsync_ConsecutiveTimeSlots_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new BookingService(context);
        var baseStart = new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc);
        var baseEnd = new DateTime(2026, 9, 15, 11, 0, 0, DateTimeKind.Utc);

        context.Bookings.Add(new Booking
        {
            Id = 1,
            ResourceId = 1,
            UserId = "user1",
            StartTime = baseStart,
            EndTime = baseEnd,
            Status = BookingStatus.Approved,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act - Slot ending right when existing starts: 09:00 - 10:00
        bool beforeSlotAvailable = await service.IsResourceAvailableAsync(
            1,
            new DateTime(2026, 9, 15, 9, 0, 0, DateTimeKind.Utc),
            baseStart);

        // Act - Slot starting right when existing ends: 11:00 - 12:00
        bool afterSlotAvailable = await service.IsResourceAvailableAsync(
            1,
            baseEnd,
            new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc));

        // Assert
        Assert.True(beforeSlotAvailable);
        Assert.True(afterSlotAvailable);
    }

    [Fact]
    public async Task IsResourceAvailableAsync_CancelledOrRejectedBooking_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new BookingService(context);
        var slotStart = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        var slotEnd = new DateTime(2026, 9, 15, 15, 0, 0, DateTimeKind.Utc);

        // Existing cancelled booking in this slot
        context.Bookings.Add(new Booking
        {
            Id = 1,
            ResourceId = 1,
            UserId = "user1",
            StartTime = slotStart,
            EndTime = slotEnd,
            Status = BookingStatus.Cancelled,
            CreatedAt = DateTime.UtcNow
        });

        // Another rejected booking in an overlapping slot
        context.Bookings.Add(new Booking
        {
            Id = 2,
            ResourceId = 1,
            UserId = "user2",
            StartTime = slotStart.AddMinutes(15),
            EndTime = slotEnd.AddMinutes(15),
            Status = BookingStatus.Rejected,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act - Attempt to book the slot: 14:00 - 15:00
        bool isAvailable = await service.IsResourceAvailableAsync(1, slotStart, slotEnd);

        // Assert
        Assert.True(isAvailable);
    }

    [Fact]
    public async Task CreateBookingAsync_OverlappingSlot_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new BookingService(context);
        var start = new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 9, 15, 11, 0, 0, DateTimeKind.Utc);

        await service.CreateBookingAsync(new CreateBookingDto
        {
            ResourceId = 1,
            UserId = "user1",
            StartTime = start,
            EndTime = end
        });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(new CreateBookingDto
            {
                ResourceId = 1,
                UserId = "user2",
                StartTime = start.AddMinutes(30),
                EndTime = end.AddMinutes(30)
            }));

        Assert.Contains("not available", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
