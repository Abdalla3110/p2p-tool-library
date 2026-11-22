using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public KycStatusEnum KycStatus { get; set; }

    public string? KycNationalId { get; set; }

    public string? KycIdPhotoUrl { get; set; }

    public string? KycSelfiePhotoUrl { get; set; }

    public int? KycReviewedByUserId { get; set; }

    public DateTime? KycReviewedAt { get; set; }

    public int TrustScore { get; set; }

    public decimal RatingAverage { get; set; }

    public int ReviewCount { get; set; }

    public decimal? OnTimeReturnRate { get; set; }

    public MembershipTierEnum Tier { get; set; }

    public bool IsVerified { get; set; }

    public bool IsActive { get; set; }

    public int? ZoneId { get; set; }

    public string? ReferralCode { get; set; }

    public bool EmailNotificationsEnabled { get; set; }

    public bool SmsNotificationsEnabled { get; set; }

    public bool PushNotificationsEnabled { get; set; }

    public decimal WalletBalance { get; set; }

    public DateTime JoinedAt { get; set; }

    public DateTime? LastActiveAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Booking> BookingBorrowerUsers { get; set; } = new List<Booking>();

    public virtual ICollection<Booking> BookingLenderUsers { get; set; } = new List<Booking>();

    public virtual ICollection<Broadcast> Broadcasts { get; set; } = new List<Broadcast>();

    public virtual ICollection<Conversation> ConversationUser1s { get; set; } = new List<Conversation>();

    public virtual ICollection<Conversation> ConversationUser2s { get; set; } = new List<Conversation>();

    public virtual ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<Disposal> Disposals { get; set; } = new List<Disposal>();

    public virtual ICollection<Dispute> DisputeAgainstUsers { get; set; } = new List<Dispute>();

    public virtual ICollection<Dispute> DisputeAssignedLibrarianUsers { get; set; } = new List<Dispute>();

    public virtual ICollection<Dispute> DisputeReportedByUsers { get; set; } = new List<Dispute>();

    public virtual ICollection<Handover> HandoverBorrowerUsers { get; set; } = new List<Handover>();

    public virtual ICollection<Handover> HandoverLenderUsers { get; set; } = new List<Handover>();

    public virtual ICollection<Handover> HandoverVerifiedByUsers { get; set; } = new List<Handover>();

    public virtual ICollection<InsuranceClaim> InsuranceClaimAssignedLibrarianUsers { get; set; } = new List<InsuranceClaim>();

    public virtual ICollection<InsuranceClaim> InsuranceClaimClaimantUsers { get; set; } = new List<InsuranceClaim>();

    public virtual ICollection<InventoryAudit> InventoryAuditOwnerUsers { get; set; } = new List<InventoryAudit>();

    public virtual ICollection<InventoryAudit> InventoryAuditReviewedByUsers { get; set; } = new List<InventoryAudit>();

    public virtual ICollection<User> InverseKycReviewedByUser { get; set; } = new List<User>();

    public virtual User? KycReviewedByUser { get; set; }

    public virtual ICollection<LateReturn> LateReturns { get; set; } = new List<LateReturn>();

    public virtual ICollection<LibrarianTask> LibrarianTasks { get; set; } = new List<LibrarianTask>();

    public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PlatformSetting> PlatformSettings { get; set; } = new List<PlatformSetting>();

    public virtual ICollection<Rating> RatingRatedUsers { get; set; } = new List<Rating>();

    public virtual ICollection<Rating> RatingRaterUsers { get; set; } = new List<Rating>();

    public virtual ICollection<Referral> ReferralReferredUsers { get; set; } = new List<Referral>();

    public virtual ICollection<Referral> ReferralReferrerUsers { get; set; } = new List<Referral>();

    public virtual Role Role { get; set; } = null!;

    public virtual TechnicianProfile? TechnicianProfile { get; set; }

    public virtual ICollection<Tool> Tools { get; set; } = new List<Tool>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<UserSuspension> UserSuspensionCreatedByUsers { get; set; } = new List<UserSuspension>();

    public virtual ICollection<UserSuspension> UserSuspensionLiftedByUsers { get; set; } = new List<UserSuspension>();

    public virtual ICollection<UserSuspension> UserSuspensionUsers { get; set; } = new List<UserSuspension>();

    public virtual Zone? Zone { get; set; }

    public virtual ICollection<Zone> Zones { get; set; } = new List<Zone>();

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiresAt { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
}
