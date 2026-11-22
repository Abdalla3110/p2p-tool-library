using DAL.Models.Entities;

namespace BLL.Interfaces;

public interface IDisputeService
{
    Task<Dispute> OpenDisputeAsync(int bookingId, int openedByUserId, DisputeTypeEnum type,
        string description, int? againstUserId = null, string? damagePhotoUrl = null);
    Task<List<Dispute>> GetMyDisputesAsync(int userId);
}
