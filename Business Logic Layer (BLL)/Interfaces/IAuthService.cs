using DAL.Models.Entities;

namespace BLL.Interfaces;

public interface IAuthService
{
    Task<User?> ValidateCredentialsAsync(string email, string password);
    Task<string?> RegisterAsync(string fullName, string? phone, string email, string password, string? referralCode);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByResetTokenAsync(string token);
    Task<User?> FindByEmailVerificationTokenAsync(string token);
    Task SaveUserAsync(User user);
    Task TouchLastActiveAsync(int userId);
}
