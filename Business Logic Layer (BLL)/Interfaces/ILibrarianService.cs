using DAL.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public class LibraryDashboard
{
    public int OpenDisputes { get; set; }
    public int PendingAudits { get; set; }
    public int PendingTools { get; set; }
    public int PendingKyc { get; set; }
    public int TodayHandovers { get; set; }
    public int OpenLateReturns { get; set; }
    public List<LibrarianTask> MyTasks { get; set; } = new();
    public List<Dispute> RecentDisputes { get; set; } = new();
}

public interface ILibrarianService
{
    Task<bool> ConfirmBookingAsync(int bookingId);
    Task<(bool Ok, string? Error)> VerifyHandoverAsync(int handoverId, string qrCode,
        bool matchesListing, bool accessories, bool noDamage, bool bothPresent,
        IFormFile? pickupPhoto, IFormFile? returnPhoto);
    Task ResolveDisputeAsync(int disputeId, DisputeResolutionEnum resolution, string? notes, decimal? refundAmount);

    // dashboard + tasks
    Task<LibraryDashboard> GetDashboardAsync(int librarianUserId);
    Task<List<LibrarianTask>> GetMyTasksAsync(int librarianUserId);
    Task<bool> CompleteTaskAsync(int taskId, int librarianUserId);

    // disputes queue
    Task<List<Dispute>> GetDisputesAsync(string? status);
    Task<Dispute?> GetDisputeForReviewAsync(int disputeId, int librarianUserId);

    // inventory audits
    Task<List<InventoryAudit>> GetAuditsAsync(string? status);
    Task<bool> ReviewAuditAsync(int auditId, string decision, string? notes, int reviewerUserId);

    // handovers
    Task<List<Handover>> GetHandoversAsync();
    Task StampHandoverVerifierAsync(int handoverId, int staffUserId);

    // insurance claims
    Task<List<InsuranceClaim>> GetClaimsAsync(string? status);
    Task ReviewClaimAsync(int claimId, bool approve, string? notes, decimal? payout);

    // late returns
    Task<List<LateReturn>> GetLateReturnsAsync(string? status);
    Task ResolveLateReturnAsync(int lateReturnId);

    // broadcasts
    Task<List<Broadcast>> GetBroadcastsAsync();
    Task<int> SendBroadcastAsync(int? zoneId, string title, string message, string priority, int senderUserId);
}
