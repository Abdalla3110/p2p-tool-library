using DAL.Models.Entities;
using DAL.Models.ViewModels;

namespace BLL.Interfaces;

public class ClaimResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public interface IClaimService
{
    Task<List<InsuranceClaim>> GetMyClaimsAsync(int userId);
    Task<ClaimResult> SubmitAsync(InsuranceClaimFormViewModel model, int userId);
}
