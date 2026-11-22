using System;
using System.Collections.Generic;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Broadcast> Broadcasts { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<DamageReport> DamageReports { get; set; }

    public virtual DbSet<Deposit> Deposits { get; set; }

    public virtual DbSet<Disposal> Disposals { get; set; }

    public virtual DbSet<Dispute> Disputes { get; set; }

    public virtual DbSet<ExternalRepair> ExternalRepairs { get; set; }

    public virtual DbSet<Handover> Handovers { get; set; }

    public virtual DbSet<InsuranceClaim> InsuranceClaims { get; set; }

    public virtual DbSet<InventoryAudit> InventoryAudits { get; set; }

    public virtual DbSet<LateReturn> LateReturns { get; set; }

    public virtual DbSet<LibrarianTask> LibrarianTasks { get; set; }

    public virtual DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PlatformSetting> PlatformSettings { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Referral> Referrals { get; set; }

    public virtual DbSet<RevenueReport> RevenueReports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TechnicianProfile> TechnicianProfiles { get; set; }

    public virtual DbSet<Tool> Tools { get; set; }

    public virtual DbSet<ToolAvailability> ToolAvailabilities { get; set; }

    public virtual DbSet<ToolCategory> ToolCategories { get; set; }

    public virtual DbSet<ToolPhoto> ToolPhotos { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSuspension> UserSuspensions { get; set; }

    public virtual DbSet<Zone> Zones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

}

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
