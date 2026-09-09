using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.API.Data;
using ReservationSystem.API.DTOs;
using ReservationSystem.API.Enums;
using ReservationSystem.API.Models;

namespace ReservationSystem.API.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto dto)
    {
        if (dto.StartTime >= dto.EndTime)
        {
            throw new ArgumentException("StartTime must be before EndTime.");
        }

        bool isAvailable = await IsResourceAvailableAsync(dto.ResourceId, dto.StartTime, dto.EndTime);
        if (!isAvailable)
        {
            throw new InvalidOperationException("Resource is not available for the selected time period.");
        }

        var booking = new Booking
        {
            ResourceId = dto.ResourceId,
            UserId = dto.UserId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return MapToDto(booking);
    }

    public async Task<IEnumerable<BookingResponseDto>> GetBookingsAsync()
    {
        var bookings = await _context.Bookings
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(MapToDto);
    }

    public async Task<bool> IsResourceAvailableAsync(int resourceId, DateTime start, DateTime end)
    {
        var overlappingBooking = await _context.Bookings
            .Where(b => b.ResourceId == resourceId)
            .Where(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Rejected)
            .Where(b => b.StartTime < end && b.EndTime > start)
            .FirstOrDefaultAsync();

        return overlappingBooking == null;
    }

    private static BookingResponseDto MapToDto(Booking booking)
    {
        return new BookingResponseDto
        {
            Id = booking.Id,
            ResourceId = booking.ResourceId,
            UserId = booking.UserId,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt
        };
    }
}
