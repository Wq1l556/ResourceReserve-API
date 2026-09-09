using System;

namespace ReservationSystem.API.DTOs;

public class CreateBookingDto
{
    public int ResourceId { get; set; }
    public required string UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
