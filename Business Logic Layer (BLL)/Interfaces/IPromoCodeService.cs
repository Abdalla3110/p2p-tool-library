using DAL.Models.Entities;

namespace BLL.Interfaces;

public class PromoResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public decimal Discount { get; set; }
}

public interface IPromoCodeService
{
    Task<PromoResult> ApplyAsync(int bookingId, string code, int userId);
    Task RemoveAsync(int bookingId, int userId);
    Task<List<Promotion>> GetAvailableAsync();
}
