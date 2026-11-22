using DAL.Models.Entities;
using DAL.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public class MemberDashboard
{
    public User? User { get; set; }
    public int MyToolsCount { get; set; }
    public int ActiveLoans { get; set; }
    public int ActiveLends { get; set; }
    public List<Booking> Upcoming { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
}

public interface IUserService
{
    Task<User?> GetProfileAsync(int userId);
    Task<User?> GetPublicProfileAsync(int userId);
    Task<MemberDashboard> GetDashboardAsync(int userId);
    Task UpdateProfileAsync(int userId, ProfileViewModel model, string? kycIdPath, string? kycSelfiePath, string? kycNationalId);
    Task<string?> SaveKycPhotoAsync(int userId, IFormFile file, string prefix);
    Task<(bool Ok, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<decimal> TopUpAsync(int userId, decimal amount);
    Task<(List<Rating> Ratings, double Average)> GetRatingsAsync(int userId);
    Task<(int Count, decimal Earned)?> GetReferralStatsAsync(int userId);
    Task<List<Referral>> GetReferralListAsync(int userId);
    Task<decimal> GetWalletBalanceAsync(int userId);
    Task<(List<Transaction> Transactions, decimal Balance)?> GetWalletAsync(int userId);

    // library admin: users, KYC, suspensions
    Task<List<User>> GetUsersForKycAsync(string? kyc);
    Task ReviewKycAsync(int userId, bool approve, int reviewerUserId);
    Task<List<UserSuspension>> GetSuspensionsAsync();
    Task<List<User>> GetActiveUsersAsync();
    Task<string?> SuspendUserAsync(int userId, SuspensionReasonEnum reason, string? description, int? days, int staffUserId);
    Task LiftSuspensionAsync(int suspensionId, int staffUserId);
}
