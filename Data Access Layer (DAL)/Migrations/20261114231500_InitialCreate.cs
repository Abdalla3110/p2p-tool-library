using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RevenueReports",
                columns: table => new
                {
                    RevenueReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionCount = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SharePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RevenueR__86917D6AB82AA3EC", x => x.RevenueReportId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__8AFACE1ADB9DFC50", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "ToolCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ToolCate__19093A0B1FE1825F", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    PromotionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DiscountType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Percentage"),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    MaxUses = table.Column<int>(type: "int", nullable: true),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Scheduled"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Promotio__52C42FCFBCC24D21", x => x.PromotionId);
                    table.ForeignKey(
                        name: "FK_Promotions_Category",
                        column: x => x.CategoryId,
                        principalTable: "ToolCategories",
                        principalColumn: "CategoryId");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AuditLog__EB5F6CBDD8B9AFFC", x => x.AuditLogId);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    BorrowerUserId = table.Column<int>(type: "int", nullable: false),
                    LenderUserId = table.Column<int>(type: "int", nullable: false),
                    PickupDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReturnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PickupTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ReturnTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    PickupLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ReturnLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PickupNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RentalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InsuranceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WalletCreditApplied = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalAmountDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PromotionId = table.Column<int>(type: "int", nullable: true),
                    PromotionDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Bookings__73951AED97635D39", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_Promotion",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "PromotionId");
                });

            migrationBuilder.CreateTable(
                name: "Broadcasts",
                columns: table => new
                {
                    BroadcastId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Normal"),
                    RecipientCount = table.Column<int>(type: "int", nullable: false),
                    SentByUserId = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Broadcas__94A16DC015CE2887", x => x.BroadcastId);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    User1Id = table.Column<int>(type: "int", nullable: false),
                    User2Id = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Conversa__C050D877DCDF21EE", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_Conversations_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "DamageReports",
                columns: table => new
                {
                    DamageReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Submitted"),
                    DisputeId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DamageRe__E448C0B5772813BD", x => x.DamageReportId);
                    table.ForeignKey(
                        name: "FK_DamageReports_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "Deposits",
                columns: table => new
                {
                    DepositId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Held"),
                    HeldAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedToUserId = table.Column<int>(type: "int", nullable: true),
                    ForfeitReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Deposits__AB60DF71393B7681", x => x.DepositId);
                    table.ForeignKey(
                        name: "FK_Deposits_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "Disposals",
                columns: table => new
                {
                    DisposalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReasonDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecycleCenter = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Photos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelFutureBookings = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisposedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Disposal__206044232569FE92", x => x.DisposalId);
                });

            migrationBuilder.CreateTable(
                name: "Disputes",
                columns: table => new
                {
                    DisputeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: false),
                    AgainstUserId = table.Column<int>(type: "int", nullable: true),
                    AgainstType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "User"),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PickupPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReturnPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DamagePhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DepositAtRisk = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    Resolution = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AssignedLibrarianUserId = table.Column<int>(type: "int", nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Disputes__7A5F68AB7460C994", x => x.DisputeId);
                    table.ForeignKey(
                        name: "FK_Disputes_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "ExternalRepairs",
                columns: table => new
                {
                    ExternalRepairId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceRecordId = table.Column<int>(type: "int", nullable: true),
                    RepairCenterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProblemDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentAt = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpectedReturnAt = table.Column<DateOnly>(type: "date", nullable: true),
                    ActualReturnAt = table.Column<DateOnly>(type: "date", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InvoiceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Inspecting"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__External__7D8FAB86F0CB7F4A", x => x.ExternalRepairId);
                });

            migrationBuilder.CreateTable(
                name: "Handovers",
                columns: table => new
                {
                    HandoverId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    BorrowerUserId = table.Column<int>(type: "int", nullable: false),
                    LenderUserId = table.Column<int>(type: "int", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    QrCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Pickup"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Scheduled"),
                    PickupPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReturnPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChecklistMatchesListing = table.Column<bool>(type: "bit", nullable: true),
                    ChecklistAccessories = table.Column<bool>(type: "bit", nullable: true),
                    ChecklistNoDamage = table.Column<bool>(type: "bit", nullable: true),
                    ChecklistBothPresent = table.Column<bool>(type: "bit", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Handover__DB2A1F815038847E", x => x.HandoverId);
                    table.ForeignKey(
                        name: "FK_Handovers_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "InsuranceClaims",
                columns: table => new
                {
                    ClaimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    ClaimantUserId = table.Column<int>(type: "int", nullable: false),
                    ToolValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IncidentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DamagePhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PoliceReportUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BookingRecordUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    InsuranceReportGenerated = table.Column<bool>(type: "bit", nullable: false),
                    DecisionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayoutAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AssignedLibrarianUserId = table.Column<int>(type: "int", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UnderReviewAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Insuranc__EF2E139B36E583EC", x => x.ClaimId);
                    table.ForeignKey(
                        name: "FK_Claims_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "InventoryAudits",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    OwnerUserId = table.Column<int>(type: "int", nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    ToolPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DatePaperPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewDecision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Inventor__A17F23988971B1E5", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "LateReturns",
                columns: table => new
                {
                    LateReturnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    BorrowerUserId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ActualReturnDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DelayDays = table.Column<int>(type: "int", nullable: false),
                    GraceHoursUsed = table.Column<int>(type: "int", nullable: false),
                    PenaltyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Warning"),
                    WarningSent = table.Column<bool>(type: "bit", nullable: false),
                    WarningSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountFrozen = table.Column<bool>(type: "bit", nullable: false),
                    FrozenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LateRetu__77CE18EB71F239D1", x => x.LateReturnId);
                    table.ForeignKey(
                        name: "FK_LateReturns_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "LibrarianTasks",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LibrarianUserId = table.Column<int>(type: "int", nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RelatedEntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Normal"),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Libraria__7C6949B1012CF692", x => x.TaskId);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    MaintenanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    TechnicianUserId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Scheduled"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationHours = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Maintena__E60542D5D77CA1BD", x => x.MaintenanceId);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderUserId = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Messages__C87C0C9CA1AD4F1C", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Messages_Conversation",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Normal"),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__20CF2E122F0B1C9F", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "PlatformSettings",
                columns: table => new
                {
                    SettingsId = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    PlatformCommissionPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 18m),
                    InsurancePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 5m),
                    DefaultDepositPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 20m),
                    DefaultMaxBorrowDays = table.Column<int>(type: "int", nullable: false, defaultValue: 7),
                    DefaultCleaningBufferHours = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    LateReturnGraceHours = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    LateReturnPenaltyPerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 50m),
                    BookingAutoApproveEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EmailBookingConfirmationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SmsLateReturnAlertsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PushNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    WeeklyMaintenanceRemindersEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PlatformPaused = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Platform__991B19FCE7597595", x => x.SettingsId);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    RatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: true),
                    RaterUserId = table.Column<int>(type: "int", nullable: false),
                    RatedUserId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ratings__FCCDF87C03C3016B", x => x.RatingId);
                    table.ForeignKey(
                        name: "FK_Ratings_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    ReferralId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferrerUserId = table.Column<int>(type: "int", nullable: false),
                    ReferredUserId = table.Column<int>(type: "int", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Referral__A2C4A96696B80405", x => x.ReferralId);
                });

            migrationBuilder.CreateTable(
                name: "TechnicianProfiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Specializations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedZoneIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToolsRepairedCount = table.Column<int>(type: "int", nullable: false),
                    AvgRepairTimeDays = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SuccessRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Rating = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OpenTasksCount = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Technici__1788CC4C85D5E2CE", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ToolAvailability",
                columns: table => new
                {
                    AvailabilityId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    BookingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ToolAvai__DA3979B1C64F8263", x => x.AvailabilityId);
                    table.ForeignKey(
                        name: "FK_Availability_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "ToolPhotos",
                columns: table => new
                {
                    PhotoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ToolPhot__21B7B5E28A2802DC", x => x.PhotoId);
                });

            migrationBuilder.CreateTable(
                name: "Tools",
                columns: table => new
                {
                    ToolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerUserId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Good"),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Available"),
                    PricePerHour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricePerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricePerWeek = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InsurancePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 5m),
                    MaxBorrowDays = table.Column<int>(type: "int", nullable: false, defaultValue: 7),
                    CleaningBufferHours = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    BatteryHealthPercentage = table.Column<int>(type: "int", nullable: true),
                    UsageHours = table.Column<int>(type: "int", nullable: false),
                    TotalRentals = table.Column<int>(type: "int", nullable: false),
                    WeeklyMaintenanceThreshold = table.Column<int>(type: "int", nullable: true),
                    LastServiceAt = table.Column<DateOnly>(type: "date", nullable: true),
                    WarrantyExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SafetyCertificateExpiry = table.Column<DateOnly>(type: "date", nullable: true),
                    SafetyCertificateUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserManualUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SafetyVideoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RatingAverage = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    IsHighValue = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tools__CC0CEB91FA573A2C", x => x.ToolId);
                    table.ForeignKey(
                        name: "FK_Tools_Category",
                        column: x => x.CategoryId,
                        principalTable: "ToolCategories",
                        principalColumn: "CategoryId");
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Completed"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Transact__55433A6B0B461929", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KycStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    KycNationalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KycIdPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KycSelfiePhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KycReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    KycReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrustScore = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    RatingAverage = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    OnTimeReturnRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Tier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Member"),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ZoneId = table.Column<int>(type: "int", nullable: true),
                    ReferralCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmailNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SmsNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PushNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    WalletBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    LastActiveAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4C391143B9", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_KycReviewer",
                        column: x => x.KycReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Users_Roles",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId");
                });

            migrationBuilder.CreateTable(
                name: "UserSuspensions",
                columns: table => new
                {
                    SuspensionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReasonDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    LiftedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LiftedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserSusp__AB37704BA22B3225", x => x.SuspensionId);
                    table.ForeignKey(
                        name: "FK_Suspensions_Creator",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Suspensions_Lifter",
                        column: x => x.LiftedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Suspensions_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    ZoneId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LibrarianUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Zones__601667B50501D747", x => x.ZoneId);
                    table.ForeignKey(
                        name: "FK_Zones_Librarian",
                        column: x => x.LibrarianUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Created",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entity",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Borrower",
                table: "Bookings",
                column: "BorrowerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Dates",
                table: "Bookings",
                columns: new[] { "PickupDate", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Lender",
                table: "Bookings",
                column: "LenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PromotionId",
                table: "Bookings",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status",
                table: "Bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Tool",
                table: "Bookings",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Broadcasts_SentByUserId",
                table: "Broadcasts",
                column: "SentByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Broadcasts_ZoneId",
                table: "Broadcasts",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_BookingId",
                table: "Conversations",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ToolId",
                table: "Conversations",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User1",
                table: "Conversations",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User2",
                table: "Conversations",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "UX_Conversations_PairBooking",
                table: "Conversations",
                columns: new[] { "User1Id", "User2Id", "BookingId" },
                unique: true,
                filter: "([BookingId] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "UX_Conversations_PairNoBooking",
                table: "Conversations",
                columns: new[] { "User1Id", "User2Id" },
                unique: true,
                filter: "([BookingId] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_BookingId",
                table: "DamageReports",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_DisputeId",
                table: "DamageReports",
                column: "DisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_ReportedByUserId",
                table: "DamageReports",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_ToolId",
                table: "DamageReports",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_ReleasedToUserId",
                table: "Deposits",
                column: "ReleasedToUserId");

            migrationBuilder.CreateIndex(
                name: "UX_Deposits_Booking",
                table: "Deposits",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disposals_ApprovedByUserId",
                table: "Disposals",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Disposals_ToolId",
                table: "Disposals",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_AgainstUserId",
                table: "Disputes",
                column: "AgainstUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_AssignedLibrarianUserId",
                table: "Disputes",
                column: "AssignedLibrarianUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_Booking",
                table: "Disputes",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_ReportedByUserId",
                table: "Disputes",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_Status",
                table: "Disputes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_ToolId",
                table: "Disputes",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalRepairs_MaintenanceRecordId",
                table: "ExternalRepairs",
                column: "MaintenanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalRepairs_ToolId",
                table: "ExternalRepairs",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Handovers_Booking",
                table: "Handovers",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Handovers_BorrowerUserId",
                table: "Handovers",
                column: "BorrowerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Handovers_LenderUserId",
                table: "Handovers",
                column: "LenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Handovers_QrCode",
                table: "Handovers",
                column: "QrCode");

            migrationBuilder.CreateIndex(
                name: "IX_Handovers_ToolId",
                table: "Handovers",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Handovers_VerifiedByUserId",
                table: "Handovers",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Handover__EE32FA443491A866",
                table: "Handovers",
                column: "QrCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_AssignedLibrarianUserId",
                table: "InsuranceClaims",
                column: "AssignedLibrarianUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_BookingId",
                table: "InsuranceClaims",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_ClaimantUserId",
                table: "InsuranceClaims",
                column: "ClaimantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_ToolId",
                table: "InsuranceClaims",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_Status",
                table: "InventoryAudits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_OwnerUserId",
                table: "InventoryAudits",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_ReviewedByUserId",
                table: "InventoryAudits",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_ToolId",
                table: "InventoryAudits",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_ZoneId",
                table: "InventoryAudits",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_LateReturns_BookingId",
                table: "LateReturns",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_LateReturns_BorrowerUserId",
                table: "LateReturns",
                column: "BorrowerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LateReturns_Status",
                table: "LateReturns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LateReturns_ToolId",
                table: "LateReturns",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_LibrarianTasks_ZoneId",
                table: "LibrarianTasks",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Librarian",
                table: "LibrarianTasks",
                columns: new[] { "LibrarianUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Maintenance_Tool",
                table: "MaintenanceRecords",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_TechnicianUserId",
                table: "MaintenanceRecords",
                column: "TechnicianUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Conversation",
                table: "Messages",
                columns: new[] { "ConversationId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderUserId",
                table: "Messages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_User",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformSettings_UpdatedByUserId",
                table: "PlatformSettings",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_CategoryId",
                table: "Promotions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "UQ__Promotio__A25C5AA7766F56A5",
                table: "Promotions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_RatedUserId",
                table: "Ratings",
                column: "RatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_RaterUserId",
                table: "Ratings",
                column: "RaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ToolId",
                table: "Ratings",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "UX_Ratings_BookingRaterNoTarget",
                table: "Ratings",
                columns: new[] { "BookingId", "RaterUserId" },
                unique: true,
                filter: "([RatedUserId] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "UX_Ratings_BookingRaterTarget",
                table: "Ratings",
                columns: new[] { "BookingId", "RaterUserId", "RatedUserId" },
                unique: true,
                filter: "([RatedUserId] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferredUserId",
                table: "Referrals",
                column: "ReferredUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferrerUserId",
                table: "Referrals",
                column: "ReferrerUserId");

            migrationBuilder.CreateIndex(
                name: "UQ_RevenueReports_Period_Source",
                table: "RevenueReports",
                columns: new[] { "PeriodMonth", "Source" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__737584F623DBBC03",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Availability_Date",
                table: "ToolAvailability",
                columns: new[] { "Date", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ToolAvailability_BookingId",
                table: "ToolAvailability",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "UQ_ToolAvailability",
                table: "ToolAvailability",
                columns: new[] { "ToolId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__ToolCate__737584F65D5829D8",
                table: "ToolCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolPhotos_ToolId",
                table: "ToolPhotos",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_Category",
                table: "Tools",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_Owner",
                table: "Tools",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_Status",
                table: "Tools",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_Zone",
                table: "Tools",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Booking",
                table: "Transactions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_User",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_KycReviewedByUserId",
                table: "Users",
                column: "KycReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ZoneId",
                table: "Users",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__7E0678120DAA43CF",
                table: "Users",
                column: "ReferralCode",
                unique: true,
                filter: "[ReferralCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D1053481C0BA0E",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suspensions_User",
                table: "UserSuspensions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSuspensions_CreatedByUserId",
                table: "UserSuspensions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSuspensions_LiftedByUserId",
                table: "UserSuspensions",
                column: "LiftedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Zones_LibrarianUserId",
                table: "Zones",
                column: "LibrarianUserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Zones__737584F6598D9777",
                table: "Zones",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_User",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Borrower",
                table: "Bookings",
                column: "BorrowerUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Lender",
                table: "Bookings",
                column: "LenderUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Tool",
                table: "Bookings",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Broadcasts_Sender",
                table: "Broadcasts",
                column: "SentByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Broadcasts_Zone",
                table: "Broadcasts",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Tool",
                table: "Conversations",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_User1",
                table: "Conversations",
                column: "User1Id",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_User2",
                table: "Conversations",
                column: "User2Id",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageReports_Dispute",
                table: "DamageReports",
                column: "DisputeId",
                principalTable: "Disputes",
                principalColumn: "DisputeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageReports_Tool",
                table: "DamageReports",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageReports_User",
                table: "DamageReports",
                column: "ReportedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deposits_ReleasedTo",
                table: "Deposits",
                column: "ReleasedToUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Disposals_Approver",
                table: "Disposals",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Disposals_Tool",
                table: "Disposals",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_Against",
                table: "Disputes",
                column: "AgainstUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_Librarian",
                table: "Disputes",
                column: "AssignedLibrarianUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_Reporter",
                table: "Disputes",
                column: "ReportedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_Tool",
                table: "Disputes",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtRepairs_Maint",
                table: "ExternalRepairs",
                column: "MaintenanceRecordId",
                principalTable: "MaintenanceRecords",
                principalColumn: "MaintenanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtRepairs_Tool",
                table: "ExternalRepairs",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Handovers_Borrower",
                table: "Handovers",
                column: "BorrowerUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Handovers_Lender",
                table: "Handovers",
                column: "LenderUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Handovers_Verifier",
                table: "Handovers",
                column: "VerifiedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Handovers_Tool",
                table: "Handovers",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Claimant",
                table: "InsuranceClaims",
                column: "ClaimantUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Librarian",
                table: "InsuranceClaims",
                column: "AssignedLibrarianUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Tool",
                table: "InsuranceClaims",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Audits_Owner",
                table: "InventoryAudits",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Audits_Reviewer",
                table: "InventoryAudits",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Audits_Tool",
                table: "InventoryAudits",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Audits_Zone",
                table: "InventoryAudits",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_LateReturns_Borrower",
                table: "LateReturns",
                column: "BorrowerUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LateReturns_Tool",
                table: "LateReturns",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Librarian",
                table: "LibrarianTasks",
                column: "LibrarianUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Zone",
                table: "LibrarianTasks",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenance_Tech",
                table: "MaintenanceRecords",
                column: "TechnicianUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenance_Tool",
                table: "MaintenanceRecords",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Sender",
                table: "Messages",
                column: "SenderUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_User",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_Updater",
                table: "PlatformSettings",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Rated",
                table: "Ratings",
                column: "RatedUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Rater",
                table: "Ratings",
                column: "RaterUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Tool",
                table: "Ratings",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Referred",
                table: "Referrals",
                column: "ReferredUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Referrer",
                table: "Referrals",
                column: "ReferrerUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TechProfiles_User",
                table: "TechnicianProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Availability_Tool",
                table: "ToolAvailability",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToolPhotos_Tool",
                table: "ToolPhotos",
                column: "ToolId",
                principalTable: "Tools",
                principalColumn: "ToolId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tools_Owner",
                table: "Tools",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tools_Zone",
                table: "Tools",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_User",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Zones",
                table: "Users",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "ZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Zones_Librarian",
                table: "Zones");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Broadcasts");

            migrationBuilder.DropTable(
                name: "DamageReports");

            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropTable(
                name: "Disposals");

            migrationBuilder.DropTable(
                name: "ExternalRepairs");

            migrationBuilder.DropTable(
                name: "Handovers");

            migrationBuilder.DropTable(
                name: "InsuranceClaims");

            migrationBuilder.DropTable(
                name: "InventoryAudits");

            migrationBuilder.DropTable(
                name: "LateReturns");

            migrationBuilder.DropTable(
                name: "LibrarianTasks");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PlatformSettings");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "RevenueReports");

            migrationBuilder.DropTable(
                name: "TechnicianProfiles");

            migrationBuilder.DropTable(
                name: "ToolAvailability");

            migrationBuilder.DropTable(
                name: "ToolPhotos");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "UserSuspensions");

            migrationBuilder.DropTable(
                name: "Disputes");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropTable(
                name: "ToolCategories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Zones");
        }
    }
}
