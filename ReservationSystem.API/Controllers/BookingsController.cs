using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationSystem.API.DTOs;
using ReservationSystem.API.Services;

namespace ReservationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
            {
                dto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
            }
            var booking = await _bookingService.CreateBookingAsync(dto);
            return CreatedAtAction(nameof(GetBookings), new { id = booking.Id }, booking);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetBookings()
    {
        var bookings = await _bookingService.GetBookingsAsync();
        return Ok(bookings);
    }
}
