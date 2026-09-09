using System.Collections.Generic;
using System.Threading.Tasks;
using ReservationSystem.API.DTOs;

namespace ReservationSystem.API.Services;

public interface IBookingService
{
    Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto dto);
    Task<IEnumerable<BookingResponseDto>> GetBookingsAsync();
    Task<bool> IsResourceAvailableAsync(int resourceId, DateTime start, DateTime end);
}
