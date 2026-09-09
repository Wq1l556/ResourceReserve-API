using ReservationSystem.API.Enums;

namespace ReservationSystem.API.Models;

public class Resource
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ResourceType Type { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation property
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
