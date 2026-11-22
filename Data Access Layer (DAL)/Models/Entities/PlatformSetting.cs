using System;
using System.Collections.Generic;

namespace DAL.Models.Entities;

public partial class PlatformSetting
{
    public int SettingsId { get; set; }

    public decimal PlatformCommissionPercentage { get; set; }

    public decimal InsurancePercentage { get; set; }

    public decimal DefaultDepositPercentage { get; set; }

    public int DefaultMaxBorrowDays { get; set; }

    public int DefaultCleaningBufferHours { get; set; }

    public int LateReturnGraceHours { get; set; }

    public decimal LateReturnPenaltyPerDay { get; set; }

    public bool BookingAutoApproveEnabled { get; set; }

    public bool EmailBookingConfirmationsEnabled { get; set; }

    public bool SmsLateReturnAlertsEnabled { get; set; }

    public bool PushNotificationsEnabled { get; set; }

    public bool WeeklyMaintenanceRemindersEnabled { get; set; }

    public bool PlatformPaused { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }

    public virtual User? UpdatedByUser { get; set; }
}
