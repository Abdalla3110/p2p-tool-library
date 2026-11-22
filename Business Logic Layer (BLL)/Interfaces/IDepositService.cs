using DAL.Models.Entities;

namespace BLL.Interfaces;

public interface IDepositService
{
    Task<Deposit?> GetForBookingAsync(int bookingId);
    Task ReleaseToLenderAsync(int bookingId);
    Task RefundToBorrowerAsync(int bookingId, decimal amount);
    Task SplitAsync(int bookingId, decimal borrowerAmount, decimal lenderAmount);
}
