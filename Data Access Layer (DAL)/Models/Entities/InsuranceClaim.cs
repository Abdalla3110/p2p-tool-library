using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class InsuranceClaim
{
    public int ClaimId { get; set; }

    public int BookingId { get; set; }

    public int ToolId { get; set; }

    public int ClaimantUserId { get; set; }

    public decimal? ToolValue { get; set; }

    public DateOnly? IncidentDate { get; set; }

    public string? Description { get; set; }

    public string? DamagePhotoUrl { get; set; }

    public string? PoliceReportUrl { get; set; }

    public string? BookingRecordUrl { get; set; }

    public InsuranceClaimStatusEnum Status { get; set; }

    public bool InsuranceReportGenerated { get; set; }

    public string? DecisionNotes { get; set; }

    public decimal? PayoutAmount { get; set; }

    public int? AssignedLibrarianUserId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public DateTime? UnderReviewAt { get; set; }

    public DateTime? DecidedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? AssignedLibrarianUser { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User ClaimantUser { get; set; } = null!;

    public virtual Tool Tool { get; set; } = null!;
}
