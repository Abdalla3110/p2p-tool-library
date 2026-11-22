using DAL.Models.Entities;

namespace BLL.Interfaces;

public class BookingResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Booking? Booking { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal FeeAmount { get; set; }
}

public interface IReservationService
{
    Task<BookingResult> CreateBookingAsync(int toolId, int borrowerId, DateTime pickupDate, DateTime returnDate, Func<int, string> bookingUrlFactory);
    Task<BookingResult> PayBookingAsync(int bookingId, int userId, Func<int, string> bookingUrlFactory);
    Task<BookingResult> CancelBookingAsync(int bookingId, int userId);
    Task<BookingResult> RescheduleBookingAsync(int bookingId, int userId, DateTime pickupDate, DateTime returnDate);

    // queries
    Task<List<Booking>> GetMyBookingsAsync(int userId);
    Task<Booking?> GetBookingWithPartiesAsync(int bookingId);
    Task<Booking?> GetBookingForCheckoutAsync(int bookingId);
    Task<Booking?> GetBookingForDisputeAsync(int bookingId);
    Task<Booking?> GetBookingForClaimAsync(int bookingId, bool includeZone);
    Task<Booking?> GetBookingForDamageAsync(int bookingId);
    Task<List<Booking>> GetAllBookingsAsync(string? status);
}
