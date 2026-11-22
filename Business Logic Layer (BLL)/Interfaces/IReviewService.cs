using DAL.Models.Entities;

namespace BLL.Interfaces;

public interface IReviewService
{
    Task<(bool Ok, string? Error)> RateBookingAsync(int bookingId, int raterId, int rating, string? comment);
    Task<(bool Ok, string? Error)> RateToolAsync(int toolId, int raterId, int rating, string? comment);
    Task<List<Rating>> GetReceivedAsync(int userId);
    Task<List<Rating>> GetGivenAsync(int userId);
    Task<bool> HasRatedBookingAsync(int bookingId, int raterId, int ratedUserId);
}
