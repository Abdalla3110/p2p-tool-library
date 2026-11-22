using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using DAL.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    public UserService(AppDbContext db) => _db = db;

    public async Task<User?> GetProfileAsync(int userId) =>
        await _db.Users.Include(u => u.Zone).FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<User?> GetPublicProfileAsync(int userId) =>
        await _db.Users.Include(u => u.Role).Include(u => u.Zone)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

    public async Task<MemberDashboard> GetDashboardAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var activeStatuses = new[] { BookingStatusEnum.Pending, BookingStatusEnum.Confirmed, BookingStatusEnum.Active };
        return new MemberDashboard
        {
            User = await _db.Users.Include(u => u.Zone).FirstOrDefaultAsync(u => u.UserId == userId),
            MyToolsCount = await _db.Tools.CountAsync(t => t.OwnerUserId == userId && t.IsActive),
            ActiveLoans = await _db.Bookings.CountAsync(b => b.BorrowerUserId == userId && activeStatuses.Contains(b.Status)),
            ActiveLends = await _db.Bookings.CountAsync(b => b.LenderUserId == userId && activeStatuses.Contains(b.Status)),
            Upcoming = await _db.Bookings.Include(b => b.Tool)
                .Where(b => b.BorrowerUserId == userId && b.Status == BookingStatusEnum.Confirmed && b.PickupDate >= today)
                .OrderBy(b => b.PickupDate).Take(5).ToListAsync(),
            Notifications = await _db.Notifications.Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt).Take(5).ToListAsync(),
        };
    }

    private static readonly string[] KycImageExts = { ".jpg", ".jpeg", ".png", ".webp" };

    public async Task<string?> SaveKycPhotoAsync(int userId, IFormFile file, string prefix)
    {
        if (file.Length == 0 || !KycImageExts.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            return null;
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "kyc", userId.ToString());
        Directory.CreateDirectory(dir);
        var fname = $"{prefix}-{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        await using (var fs = File.Create(Path.Combine(dir, fname)))
            await file.CopyToAsync(fs);
        return $"/uploads/kyc/{userId}/{fname}";
    }

    public async Task<(bool Ok, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return (false, "NOT_FOUND");
        if (!PasswordHasher.Verify(currentPassword, user.PasswordHash)) return (false, "WRONG_PASSWORD");
        user.PasswordHash = PasswordHasher.Hash(newPassword);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task UpdateProfileAsync(int userId, ProfileViewModel model,
        string? kycIdPath, string? kycSelfiePath, string? kycNationalId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return;

        user.FullName = model.FullName.Trim();
        user.Phone = model.Phone;
        user.Bio = model.Bio;
        user.AvatarUrl = model.AvatarUrl;
        user.ZoneId = model.ZoneId;
        user.EmailNotificationsEnabled = model.EmailNotificationsEnabled;
        user.SmsNotificationsEnabled = model.SmsNotificationsEnabled;
        user.PushNotificationsEnabled = model.PushNotificationsEnabled;

        var kycChanged = false;
        if (kycIdPath is not null) { user.KycIdPhotoUrl = kycIdPath; kycChanged = true; }
        if (kycSelfiePath is not null) { user.KycSelfiePhotoUrl = kycSelfiePath; kycChanged = true; }
        if (!string.IsNullOrWhiteSpace(kycNationalId))
        {
            user.KycNationalId = kycNationalId.Trim();
            kycChanged = true;
        }
        if (kycChanged && user.KycStatus != KycStatusEnum.Verified)
            user.KycStatus = KycStatusEnum.Pending;

        await _db.SaveChangesAsync();
    }

    public async Task<decimal> TopUpAsync(int userId, decimal amount)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null || amount <= 0) return user?.WalletBalance ?? 0;
        user.WalletBalance += amount;
        _db.Transactions.Add(new Transaction
        {
            UserId = userId, Type = TransactionTypeEnum.Payment, Amount = amount,
            BalanceAfter = user.WalletBalance, Status = TransactionStatusEnum.Completed,
            Description = "Wallet top-up (simulated payment)", Reference = $"TOP-{Guid.NewGuid():N}"[..12],
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
        return user.WalletBalance;
    }

    public async Task<(List<Rating> Ratings, double Average)> GetRatingsAsync(int userId)
    {
        var ratings = await _db.Ratings
            .Where(r => r.RatedUserId == userId)
            .OrderByDescending(r => r.CreatedAt).Take(50).ToListAsync();
        var avg = ratings.Count > 0 ? ratings.Average(r => r.Rating1) : 0;
        return (ratings, avg);
    }

    public async Task<(int Count, decimal Earned)?> GetReferralStatsAsync(int userId)
    {
        var refs = await _db.Referrals.Where(r => r.ReferrerUserId == userId).ToListAsync();
        if (refs.Count == 0) return null;
        return (refs.Count, refs.Sum(r => r.CreditAmount));
    }

    public async Task<List<Referral>> GetReferralListAsync(int userId) =>
        await _db.Referrals.Include(r => r.ReferredUser)
            .Where(r => r.ReferrerUserId == userId).OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<decimal> GetWalletBalanceAsync(int userId) =>
        await _db.Users.Where(u => u.UserId == userId).Select(u => u.WalletBalance).FirstOrDefaultAsync();

    public async Task<(List<Transaction> Transactions, decimal Balance)?> GetWalletAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return null;
        var txns = await _db.Transactions.Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt).Take(100).ToListAsync();
        return (txns, user.WalletBalance);
    }

    /* ---------------- library admin: users / KYC / suspensions ---------------- */

    public async Task<List<User>> GetUsersForKycAsync(string? kyc)
    {
        var q = _db.Users.Include(u => u.Role).Include(u => u.Zone).AsQueryable();
        if (!string.IsNullOrEmpty(kyc) && kyc != "All"
            && Enum.TryParse<KycStatusEnum>(kyc, out var kycStatus))
            q = q.Where(u => u.KycStatus == kycStatus);
        return await q.OrderByDescending(u => u.JoinedAt).Take(200).ToListAsync();
    }

    public async Task ReviewKycAsync(int userId, bool approve, int reviewerUserId)
    {
        var u = await _db.Users.FindAsync(userId);
        if (u is null) return;
        u.KycStatus = approve ? KycStatusEnum.Verified : KycStatusEnum.Rejected;
        u.IsVerified = approve;
        u.KycReviewedByUserId = reviewerUserId;
        u.KycReviewedAt = DateTime.UtcNow;
        _db.Notifications.Add(new Notification
        {
            UserId = u.UserId, Type = "System",
            Title = approve ? "KYC verified" : "KYC rejected",
            Message = approve ? "Your identity verification was approved." : "Your identity verification was rejected. Contact support.",
            Priority = approve ? NotificationPriorityEnum.Normal : NotificationPriorityEnum.High, CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<UserSuspension>> GetSuspensionsAsync() =>
        await _db.UserSuspensions
            .Include(s => s.User).Include(s => s.CreatedByUser)
            .OrderByDescending(s => s.CreatedAt).ToListAsync();

    public async Task<List<User>> GetActiveUsersAsync() =>
        await _db.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();

    public async Task<string?> SuspendUserAsync(int userId, SuspensionReasonEnum reason, string? description, int? days, int staffUserId)
    {
        if (userId == staffUserId) return "SELF";
        _db.UserSuspensions.Add(new UserSuspension
        {
            UserId = userId,
            Type = days.HasValue ? SuspensionTypeEnum.Temporary : SuspensionTypeEnum.Permanent,
            Reason = reason,
            ReasonDescription = description,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = days.HasValue ? DateOnly.FromDateTime(DateTime.Today.AddDays(days.Value)) : null,
            Status = SuspensionStatusEnum.Active,
            CreatedByUserId = staffUserId,
            CreatedAt = DateTime.UtcNow,
        });
        var user = await _db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.IsActive = false;
            _db.Notifications.Add(new Notification
            {
                UserId = userId, Type = "System", Title = "Account suspended",
                Message = $"Your account was suspended: {reason}. Contact support for details.",
                Priority = NotificationPriorityEnum.Emergency, CreatedAt = DateTime.UtcNow,
            });
        }
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task LiftSuspensionAsync(int suspensionId, int staffUserId)
    {
        var s = await _db.UserSuspensions.Include(x => x.User).FirstOrDefaultAsync(x => x.SuspensionId == suspensionId);
        if (s is null) return;
        s.Status = SuspensionStatusEnum.Lifted;
        s.LiftedAt = DateTime.UtcNow;
        s.LiftedByUserId = staffUserId;
        if (s.User is not null &&
            !await _db.UserSuspensions.AnyAsync(x => x.UserId == s.UserId && x.SuspensionId != suspensionId && x.Status == SuspensionStatusEnum.Active))
            s.User.IsActive = true;
        await _db.SaveChangesAsync();
    }
}
