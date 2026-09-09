using System;
using ReservationSystem.API.Enums;

namespace ReservationSystem.API.Models;

public class Booking
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    public required string UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
