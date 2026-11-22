using System.Security.Cryptography;
using BLL.Services;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

using BLL.Interfaces;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db) => _db = db;

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !user.IsActive) return null;
        if (!PasswordHasher.Verify(password, user.PasswordHash)) return null;
        return user;
    }

    /// <summary>Returns null on success, or an error message.</summary>
    public async Task<string?> RegisterAsync(string fullName, string? phone, string email,
        string password, string? referralCode)
    {
        var emailExists = await _db.Users.AnyAsync(u => u.Email == email);
        if (emailExists) return "An account with this email already exists.";

        var memberRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Member");
        if (memberRole is null) return "System roles are not seeded.";

        var user = new User
        {
            RoleId = memberRole.RoleId,
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            PasswordHash = PasswordHasher.Hash(password),
            KycStatus = KycStatusEnum.Pending,
            Tier = MembershipTierEnum.Member,
            TrustScore = 50,
            IsActive = true,
            ReferralCode = await GenerateReferralCodeAsync(fullName),
            JoinedAt = DateTime.UtcNow,
            EmailVerificationToken = Guid.NewGuid().ToString("N"),
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // link referral if the code belongs to an existing user
        if (!string.IsNullOrWhiteSpace(referralCode))
        {
            var referrer = await _db.Users.FirstOrDefaultAsync(u => u.ReferralCode == referralCode.Trim());
            if (referrer is not null && referrer.UserId != user.UserId)
            {
                _db.Referrals.Add(new Referral
                {
                    ReferrerUserId = referrer.UserId,
                    ReferredUserId = user.UserId,
                    ReferralCode = referralCode.Trim(),
                    CreditAmount = 100,
                    Status = ReferralStatusEnum.Pending,
                    CreatedAt = DateTime.UtcNow,
                });
                await _db.SaveChangesAsync();
            }
        }

        return null;
    }

    public async Task<User?> FindByEmailAsync(string email)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLowerInvariant());

    public async Task<User?> FindByResetTokenAsync(string token)
        => await _db.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == token && u.PasswordResetExpiresAt > DateTime.UtcNow);

    public async Task<User?> FindByEmailVerificationTokenAsync(string token)
        => await _db.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

    public async Task SaveUserAsync(User user) => await _db.SaveChangesAsync();

    public async Task TouchLastActiveAsync(int userId)
    {
        await _db.Users.Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastActiveAt, DateTime.UtcNow));
    }

    private async Task<string> GenerateReferralCodeAsync(string fullName)
    {
        var prefix = new string(fullName.Where(char.IsLetter).Take(4).ToArray()).ToUpperInvariant();
        if (prefix.Length == 0) prefix = "USER";
        for (var i = 0; i < 20; i++)
        {
            var code = $"{prefix}{RandomNumberGenerator.GetInt32(1000, 9999)}";
            if (!await _db.Users.AnyAsync(u => u.ReferralCode == code)) return code;
        }
        return $"{prefix}{DateTime.UtcNow.Ticks % 10000}";
    }
}
