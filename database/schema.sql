/* =====================================================================
   P2P Tool Library — SQL Server schema
   Generated from the frontend prototype (public/ folder, 62 pages)
   Target: ASP.NET Core MVC / EF Core
   ===================================================================== */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------------------------------------------------------------------
   LOOKUP TABLES
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Zones (
    ZoneId          INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(100) NOT NULL UNIQUE,          -- Nasr City, Maadi...
    LibrarianUserId INT NULL,                                -- assigned librarian (FK added after Users)
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.ToolCategories (
    CategoryId        INT IDENTITY(1,1) PRIMARY KEY,
    Name              NVARCHAR(100) NOT NULL UNIQUE,               -- Power Tools, Garden...
    Icon              NVARCHAR(50)  NULL,
    ParentCategoryId  INT NULL,                                     -- hierarchical categories
    CreatedAt         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ToolCategories_Parent FOREIGN KEY (ParentCategoryId)
        REFERENCES dbo.ToolCategories(CategoryId)
);
GO

CREATE TABLE dbo.Roles (
    RoleId      INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(50) NOT NULL UNIQUE               -- Member, Librarian, Technician, Admin
);
GO

/* ---------------------------------------------------------------------
   USERS / MEMBERS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Users (
    UserId                    INT IDENTITY(1,1) PRIMARY KEY,
    RoleId                    INT NOT NULL,
    FullName                  NVARCHAR(200) NOT NULL,
    Email                     NVARCHAR(256) NOT NULL UNIQUE,
    Phone                     NVARCHAR(50)  NULL,
    PasswordHash              NVARCHAR(500) NOT NULL,
    Bio                       NVARCHAR(MAX) NULL,
    AvatarUrl                 NVARCHAR(500) NULL,

    -- KYC
    KycStatus                 NVARCHAR(20) NOT NULL DEFAULT 'Pending'
                              CHECK (KycStatus IN ('Verified','Pending','Rejected')),
    KycNationalId             NVARCHAR(100) NULL,
    KycIdPhotoUrl             NVARCHAR(500) NULL,
    KycSelfiePhotoUrl         NVARCHAR(500) NULL,
    KycReviewedByUserId       INT NULL,
    KycReviewedAt             DATETIME2 NULL,

    -- Reputation (denormalized for fast reads)
    TrustScore                INT NOT NULL DEFAULT 50 CHECK (TrustScore BETWEEN 0 AND 100),
    RatingAverage             DECIMAL(3,2) NOT NULL DEFAULT 0,
    ReviewCount               INT NOT NULL DEFAULT 0,
    OnTimeReturnRate          DECIMAL(5,2) NULL,
    Tier                      NVARCHAR(20) NOT NULL DEFAULT 'Member'
                              CHECK (Tier IN ('Member','Pro','Staff')),
    IsVerified                BIT NOT NULL DEFAULT 0,
    IsActive                  BIT NOT NULL DEFAULT 1,

    ZoneId                    INT NULL,
    ReferralCode              NVARCHAR(50) NULL UNIQUE,

    -- preferences
    EmailNotificationsEnabled BIT NOT NULL DEFAULT 1,
    SmsNotificationsEnabled   BIT NOT NULL DEFAULT 0,
    PushNotificationsEnabled  BIT NOT NULL DEFAULT 1,

    WalletBalance             DECIMAL(18,2) NOT NULL DEFAULT 0,

    JoinedAt                  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    LastActiveAt              DATETIME2 NULL,

    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId),
    CONSTRAINT FK_Users_Zones FOREIGN KEY (ZoneId) REFERENCES dbo.Zones(ZoneId),
    CONSTRAINT FK_Users_KycReviewer FOREIGN KEY (KycReviewedByUserId) REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_Users_ZoneId   ON dbo.Users(ZoneId);
CREATE INDEX IX_Users_RoleId   ON dbo.Users(RoleId);
CREATE INDEX IX_Users_Email    ON dbo.Users(Email);
GO

-- circular FK resolved here (Zones <-> Users)
ALTER TABLE dbo.Zones
    ADD CONSTRAINT FK_Zones_Librarian FOREIGN KEY (LibrarianUserId) REFERENCES dbo.Users(UserId);
GO

/* ---------------------------------------------------------------------
   TOOLS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Tools (
    ToolId                    INT IDENTITY(1,1) PRIMARY KEY,
    OwnerUserId               INT NOT NULL,
    CategoryId                INT NOT NULL,
    ZoneId                    INT NOT NULL,
    Name                      NVARCHAR(200) NOT NULL,
    Brand                     NVARCHAR(100) NULL,
    Description               NVARCHAR(MAX) NULL,
    Condition                 NVARCHAR(20) NOT NULL DEFAULT 'Good'
                              CHECK (Condition IN ('Excellent','Good','Fair','Poor')),
    Status                    NVARCHAR(30) NOT NULL DEFAULT 'Available'
                              CHECK (Status IN ('Available','Rented','PendingApproval',
                                                'Maintenance','Locked','Unfit','Pending')),

    -- pricing (EGP)
    PricePerHour              DECIMAL(18,2) NOT NULL,
    PricePerDay               DECIMAL(18,2) NOT NULL,
    PricePerWeek              DECIMAL(18,2) NULL,
    DepositAmount             DECIMAL(18,2) NOT NULL DEFAULT 0,
    InsurancePercentage       DECIMAL(5,2)  NOT NULL DEFAULT 5,

    -- rental rules
    MaxBorrowDays             INT NOT NULL DEFAULT 7,
    CleaningBufferHours       INT NOT NULL DEFAULT 10,

    -- health / safety
    BatteryHealthPercentage   INT NULL CHECK (BatteryHealthPercentage BETWEEN 0 AND 100),
    UsageHours                INT NOT NULL DEFAULT 0,
    TotalRentals              INT NOT NULL DEFAULT 0,
    WeeklyMaintenanceThreshold INT NULL,          -- rentals/week before maintenance
    LastServiceAt             DATE NULL,
    WarrantyExpiryDate        DATE NULL,
    SafetyCertificateExpiry   DATE NULL,
    SafetyCertificateUrl      NVARCHAR(500) NULL,
    UserManualUrl             NVARCHAR(500) NULL,
    SafetyVideoUrl            NVARCHAR(500) NULL,

    -- reputation (denormalized)
    RatingAverage             DECIMAL(3,2) NOT NULL DEFAULT 0,
    ReviewCount               INT NOT NULL DEFAULT 0,

    IsHighValue               BIT NOT NULL DEFAULT 0,
    IsActive                  BIT NOT NULL DEFAULT 1,
    CreatedAt                 DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt                 DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Tools_Owner    FOREIGN KEY (OwnerUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Tools_Category FOREIGN KEY (CategoryId)  REFERENCES dbo.ToolCategories(CategoryId),
    CONSTRAINT FK_Tools_Zone     FOREIGN KEY (ZoneId)      REFERENCES dbo.Zones(ZoneId)
);
GO
CREATE INDEX IX_Tools_Owner    ON dbo.Tools(OwnerUserId);
CREATE INDEX IX_Tools_Category ON dbo.Tools(CategoryId);
CREATE INDEX IX_Tools_Zone     ON dbo.Tools(ZoneId);
CREATE INDEX IX_Tools_Status   ON dbo.Tools(Status);
GO

CREATE TABLE dbo.ToolPhotos (
    PhotoId        INT IDENTITY(1,1) PRIMARY KEY,
    ToolId         INT NOT NULL,
    Url            NVARCHAR(500) NOT NULL,
    IsPrimary      BIT NOT NULL DEFAULT 0,
    SortOrder      INT NOT NULL DEFAULT 0,
    DocumentType   NVARCHAR(20) NULL,                            -- Photo, Invoice, Warranty, Document, Other
    WarrantyExpiry DATE NULL,
    CONSTRAINT FK_ToolPhotos_Tool FOREIGN KEY (ToolId) REFERENCES dbo.Tools(ToolId) ON DELETE CASCADE
);
GO

/* ---------------------------------------------------------------------
   BOOKINGS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Bookings (
    BookingId               INT IDENTITY(1,1) PRIMARY KEY,
    ToolId                  INT NOT NULL,
    BorrowerUserId          INT NOT NULL,
    LenderUserId            INT NOT NULL,          -- = Tools.OwnerUserId (denormalized for speed)

    PickupDate              DATE NOT NULL,
    ReturnDate              DATE NOT NULL,
    PickupTime              TIME NULL,
    ReturnTime              TIME NULL,
    PickupLocation          NVARCHAR(300) NULL,
    ReturnLocation          NVARCHAR(300) NULL,
    PickupNotes             NVARCHAR(MAX) NULL,

    -- money (EGP)
    RentalAmount            DECIMAL(18,2) NOT NULL,
    InsuranceAmount         DECIMAL(18,2) NOT NULL DEFAULT 0,
    PlatformFee             DECIMAL(18,2) NOT NULL DEFAULT 0,
    DepositAmount           DECIMAL(18,2) NOT NULL DEFAULT 0,
    DepositStatus           NVARCHAR(20) NOT NULL DEFAULT 'Pending'
                            CHECK (DepositStatus IN ('Pending','Held','Released','Forfeited','Refunded')),
    TotalAmount             DECIMAL(18,2) NOT NULL,
    WalletCreditApplied     DECIMAL(18,2) NOT NULL DEFAULT 0,
    FinalAmountDue          DECIMAL(18,2) NOT NULL,

    Status                  NVARCHAR(30) NOT NULL DEFAULT 'Pending'
                            CHECK (Status IN ('Pending','Confirmed','WaitingHandover',
                                              'Active','Completed','Cancelled','Disputed')),
    CancellationReason      NVARCHAR(MAX) NULL,
    CancelledAt             DATETIME2 NULL,

    -- promotion applied
    PromotionId             INT NULL,
    PromotionDiscount       DECIMAL(18,2) NOT NULL DEFAULT 0,

    CreatedAt               DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt               DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Bookings_Tool     FOREIGN KEY (ToolId)         REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_Bookings_Borrower FOREIGN KEY (BorrowerUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Bookings_Lender   FOREIGN KEY (LenderUserId)   REFERENCES dbo.Users(UserId)
    -- FK_Bookings_Promotion added after Promotions table is created
);
GO
CREATE INDEX IX_Bookings_Tool      ON dbo.Bookings(ToolId);
CREATE INDEX IX_Bookings_Borrower  ON dbo.Bookings(BorrowerUserId);
CREATE INDEX IX_Bookings_Lender    ON dbo.Bookings(LenderUserId);
CREATE INDEX IX_Bookings_Status    ON dbo.Bookings(Status);
CREATE INDEX IX_Bookings_Dates     ON dbo.Bookings(PickupDate, ReturnDate);
GO

/* ---------------------------------------------------------------------
   HANDOVERS  (QR pickup / return verification)
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Handovers (
    HandoverId              INT IDENTITY(1,1) PRIMARY KEY,
    BookingId               INT NOT NULL,
    ToolId                  INT NOT NULL,
    BorrowerUserId          INT NOT NULL,
    LenderUserId            INT NOT NULL,

    ScheduledAt             DATETIME2 NOT NULL,
    Location                NVARCHAR(300) NULL,
    QrCode                  NVARCHAR(100) NOT NULL UNIQUE,
    Type                    NVARCHAR(10) NOT NULL DEFAULT 'Pickup'
                            CHECK (Type IN ('Pickup','Return')),
    Status                  NVARCHAR(20) NOT NULL DEFAULT 'Scheduled'
                            CHECK (Status IN ('Scheduled','PendingQR','Verified','FailedScan','Expired')),

    PickupPhotoUrl          NVARCHAR(500) NULL,
    ReturnPhotoUrl          NVARCHAR(500) NULL,

    -- verification checklist
    ChecklistMatchesListing BIT NULL,
    ChecklistAccessories    BIT NULL,
    ChecklistNoDamage       BIT NULL,
    ChecklistBothPresent    BIT NULL,

    VerifiedAt              DATETIME2 NULL,
    VerifiedByUserId        INT NULL,               -- librarian who scanned

    CreatedAt               DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Handovers_Booking FOREIGN KEY (BookingId)        REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_Handovers_Tool    FOREIGN KEY (ToolId)           REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_Handovers_Borrower FOREIGN KEY (BorrowerUserId)  REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Handovers_Lender  FOREIGN KEY (LenderUserId)     REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Handovers_Verifier FOREIGN KEY (VerifiedByUserId) REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_Handovers_Booking ON dbo.Handovers(BookingId);
CREATE INDEX IX_Handovers_QrCode  ON dbo.Handovers(QrCode);
GO

/* ---------------------------------------------------------------------
   DISPUTES  (librarian mediation)
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Disputes (
    DisputeId               INT IDENTITY(1,1) PRIMARY KEY,
    BookingId               INT NOT NULL,
    ToolId                  INT NOT NULL,
    ReportedByUserId        INT NOT NULL,
    AgainstUserId           INT NULL,               -- NULL when against the platform
    AgainstType             NVARCHAR(10) NOT NULL DEFAULT 'User'
                            CHECK (AgainstType IN ('User','Platform')),
    Type                    NVARCHAR(20) NOT NULL
                            CHECK (Type IN ('Damage','Payment','LateReturn','Other')),
    Description             NVARCHAR(MAX) NOT NULL,
    PickupPhotoUrl          NVARCHAR(500) NULL,
    ReturnPhotoUrl          NVARCHAR(500) NULL,
    DamagePhotoUrl          NVARCHAR(500) NULL,

    DepositAtRisk           DECIMAL(18,2) NULL,

    Status                  NVARCHAR(20) NOT NULL DEFAULT 'Open'
                            CHECK (Status IN ('Open','InReview','Resolved','Rejected','Reopened')),
    Resolution              NVARCHAR(30) NULL
                            CHECK (Resolution IN ('ReleaseDepositToLender','PartialRefundToBorrower',
                                                  'FullRefundToBorrower','SplitDeposit')),
    ResolutionNotes         NVARCHAR(MAX) NULL,
    RefundAmount            DECIMAL(18,2) NULL,

    AssignedLibrarianUserId INT NULL,
    OpenedAt                DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ResolvedAt              DATETIME2 NULL,
    CreatedAt               DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt               DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Disputes_Booking   FOREIGN KEY (BookingId)   REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_Disputes_Tool      FOREIGN KEY (ToolId)      REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_Disputes_Reporter  FOREIGN KEY (ReportedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Disputes_Against   FOREIGN KEY (AgainstUserId)    REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Disputes_Librarian FOREIGN KEY (AssignedLibrarianUserId) REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_Disputes_Status ON dbo.Disputes(Status);
CREATE INDEX IX_Disputes_Booking ON dbo.Disputes(BookingId);
GO

/* Damage reports filed by borrowers during active rentals */
CREATE TABLE dbo.DamageReports (
    DamageReportId      INT IDENTITY(1,1) PRIMARY KEY,
    BookingId           INT NOT NULL,
    ToolId              INT NOT NULL,
    ReportedByUserId    INT NOT NULL,
    Description         NVARCHAR(MAX) NULL,
    PhotoUrl            NVARCHAR(500) NULL,
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Submitted'
                        CHECK (Status IN ('Submitted','UnderReview','LinkedToDispute','Closed')),
    DisputeId           INT NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_DamageReports_Booking FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_DamageReports_Tool    FOREIGN KEY (ToolId)    REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_DamageReports_User    FOREIGN KEY (ReportedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_DamageReports_Dispute FOREIGN KEY (DisputeId) REFERENCES dbo.Disputes(DisputeId)
);
GO

/* ---------------------------------------------------------------------
   PAYMENTS / WALLET
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Transactions (
    TransactionId       INT IDENTITY(1,1) PRIMARY KEY,
    UserId              INT NOT NULL,
    BookingId           INT NULL,
    Type                NVARCHAR(20) NOT NULL
                        CHECK (Type IN ('Payment','Deposit','Refund','Credit','Fine','Commission','Reversal')),
    Amount              DECIMAL(18,2) NOT NULL,         -- signed: + credit, - debit
    BalanceAfter        DECIMAL(18,2) NULL,
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Completed'
                        CHECK (Status IN ('Completed','Held','Pending','Failed','Applied','Released')),
    Description         NVARCHAR(500) NULL,
    Reference           NVARCHAR(100) NULL,             -- e.g. booking code
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Transactions_User    FOREIGN KEY (UserId)    REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Transactions_Booking FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId)
);
GO
CREATE INDEX IX_Transactions_User ON dbo.Transactions(UserId);
CREATE INDEX IX_Transactions_Booking ON dbo.Transactions(BookingId);
GO

/* Escrow deposits held per booking */
CREATE TABLE dbo.Deposits (
    DepositId       INT IDENTITY(1,1) PRIMARY KEY,
    BookingId       INT NOT NULL,
    Amount          DECIMAL(18,2) NOT NULL,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'Held'
                    CHECK (Status IN ('Held','Released','Forfeited','Refunded')),
    HeldAt          DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ReleasedAt      DATETIME2 NULL,
    ReleasedToUserId INT NULL,
    ForfeitReason   NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Deposits_Booking FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_Deposits_ReleasedTo FOREIGN KEY (ReleasedToUserId) REFERENCES dbo.Users(UserId)
);
GO
CREATE UNIQUE INDEX UX_Deposits_Booking ON dbo.Deposits(BookingId); -- one escrow per booking
GO

/* ---------------------------------------------------------------------
   RATINGS & REVIEWS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Ratings (
    RatingId        INT IDENTITY(1,1) PRIMARY KEY,
    BookingId       INT NOT NULL,
    ToolId          INT NULL,
    RaterUserId     INT NOT NULL,
    RatedUserId     INT NULL,               -- borrower rates lender (per spec)
    Rating          INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment         NVARCHAR(MAX) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Ratings_Booking FOREIGN KEY (BookingId)   REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_Ratings_Tool    FOREIGN KEY (ToolId)      REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_Ratings_Rater   FOREIGN KEY (RaterUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Ratings_Rated   FOREIGN KEY (RatedUserId) REFERENCES dbo.Users(UserId)
);
GO
-- one rating per rater per booking per rated-target (handles NULL RatedUserId)
CREATE UNIQUE INDEX UX_Ratings_BookingRaterNoTarget
    ON dbo.Ratings(BookingId, RaterUserId) WHERE RatedUserId IS NULL;
CREATE UNIQUE INDEX UX_Ratings_BookingRaterTarget
    ON dbo.Ratings(BookingId, RaterUserId, RatedUserId) WHERE RatedUserId IS NOT NULL;

/* ---------------------------------------------------------------------
   INVENTORY AUDITS  (periodic tool condition checks)
   ------------------------------------------------------------------- */

CREATE TABLE dbo.InventoryAudits (
    AuditId             INT IDENTITY(1,1) PRIMARY KEY,
    ToolId              INT NOT NULL,
    OwnerUserId         INT NOT NULL,
    ZoneId              INT NOT NULL,
    DueDate             DATE NOT NULL,
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Pending'
                        CHECK (Status IN ('Pending','PhotoSubmitted','Approved','Rejected','Failed')),
    ToolPhotoUrl        NVARCHAR(500) NULL,
    DatePaperPhotoUrl   NVARCHAR(500) NULL,
    ReviewDecision      NVARCHAR(20) NULL
                        CHECK (ReviewDecision IN ('Approve','RejectRetake','Escalate')),
    ReviewNotes         NVARCHAR(MAX) NULL,
    ReviewedByUserId    INT NULL,
    ReviewedAt          DATETIME2 NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Audits_Tool   FOREIGN KEY (ToolId)      REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_Audits_Owner  FOREIGN KEY (OwnerUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Audits_Zone   FOREIGN KEY (ZoneId)      REFERENCES dbo.Zones(ZoneId),
    CONSTRAINT FK_Audits_Reviewer FOREIGN KEY (ReviewedByUserId) REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_Audits_Status ON dbo.InventoryAudits(Status);
GO

/* ---------------------------------------------------------------------
   INSURANCE CLAIMS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.InsuranceClaims (
    ClaimId                 INT IDENTITY(1,1) PRIMARY KEY,
    BookingId               INT NOT NULL,
    ToolId                  INT NOT NULL,
    ClaimantUserId          INT NOT NULL,
    ToolValue               DECIMAL(18,2) NULL,
    IncidentDate            DATE NULL,
    Description             NVARCHAR(MAX) NULL,
    DamagePhotoUrl          NVARCHAR(500) NULL,
    PoliceReportUrl         NVARCHAR(500) NULL,
    BookingRecordUrl        NVARCHAR(500) NULL,
    Status                  NVARCHAR(20) NOT NULL DEFAULT 'Open'
                            CHECK (Status IN ('Open','InReview','Approved','Rejected')),
    InsuranceReportGenerated BIT NOT NULL DEFAULT 0,
    DecisionNotes           NVARCHAR(MAX) NULL,
    PayoutAmount            DECIMAL(18,2) NULL,
    AssignedLibrarianUserId INT NULL,
    SubmittedAt             DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UnderReviewAt           DATETIME2 NULL,
    DecidedAt               DATETIME2 NULL,
    CreatedAt               DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Claims_Booking   FOREIGN KEY (BookingId)   REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_Claims_Tool      FOREIGN KEY (ToolId)      REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_Claims_Claimant  FOREIGN KEY (ClaimantUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Claims_Librarian FOREIGN KEY (AssignedLibrarianUserId) REFERENCES dbo.Users(UserId)
);
GO

/* ---------------------------------------------------------------------
   LATE RETURNS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.LateReturns (
    LateReturnId        INT IDENTITY(1,1) PRIMARY KEY,
    BookingId           INT NOT NULL,
    ToolId              INT NOT NULL,
    BorrowerUserId      INT NOT NULL,
    DueDate             DATE NOT NULL,
    ActualReturnDate    DATE NULL,
    DelayDays           INT NOT NULL DEFAULT 0,
    GraceHoursUsed      INT NOT NULL DEFAULT 0,
    PenaltyAmount       DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Warning'
                        CHECK (Status IN ('Warning','FineApplied','Frozen','AccountFrozen','Resolved')),
    WarningSent         BIT NOT NULL DEFAULT 0,
    WarningSentAt       DATETIME2 NULL,
    AccountFrozen       BIT NOT NULL DEFAULT 0,
    FrozenAt            DATETIME2 NULL,
    ResolvedAt          DATETIME2 NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LateReturns_Booking  FOREIGN KEY (BookingId)      REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_LateReturns_Tool     FOREIGN KEY (ToolId)         REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_LateReturns_Borrower FOREIGN KEY (BorrowerUserId) REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_LateReturns_Status ON dbo.LateReturns(Status);
GO

/* ---------------------------------------------------------------------
   BLACKLIST / SUSPENSIONS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.UserSuspensions (
    SuspensionId        INT IDENTITY(1,1) PRIMARY KEY,
    UserId              INT NOT NULL,
    Type                NVARCHAR(20) NOT NULL
                        CHECK (Type IN ('Temporary','Permanent')),
    Reason              NVARCHAR(30) NOT NULL
                        CHECK (Reason IN ('LateReturns','Damage','Fraud','PolicyViolation','Other')),
    ReasonDescription   NVARCHAR(MAX) NULL,
    StartDate           DATE NOT NULL,
    EndDate             DATE NULL,                    -- NULL = permanent
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Active'
                        CHECK (Status IN ('Active','Lifted','Expired','UnderReview')),
    LiftedAt            DATETIME2 NULL,
    LiftedByUserId      INT NULL,
    CreatedByUserId     INT NOT NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Suspensions_User     FOREIGN KEY (UserId)          REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Suspensions_Creator  FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Suspensions_Lifter   FOREIGN KEY (LiftedByUserId)  REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_Suspensions_User ON dbo.UserSuspensions(UserId, Status);
GO

/* ---------------------------------------------------------------------
   PROMOTIONS / BROADCASTS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Promotions (
    PromotionId         INT IDENTITY(1,1) PRIMARY KEY,
    Code                NVARCHAR(50) NOT NULL UNIQUE,       -- GARDEN30
    Description         NVARCHAR(300) NULL,
    DiscountType        NVARCHAR(10) NOT NULL DEFAULT 'Percentage'
                        CHECK (DiscountType IN ('Percentage','FixedAmount')),
    DiscountPercentage  DECIMAL(5,2) NULL,
    DiscountAmount      DECIMAL(18,2) NULL,
    CategoryId          INT NULL,                           -- NULL = all categories
    StartDate           DATE NOT NULL,
    EndDate             DATE NULL,
    MaxUses             INT NULL,
    UsedCount           INT NOT NULL DEFAULT 0,
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Scheduled'
                        CHECK (Status IN ('Scheduled','Active','Expired','Disabled')),
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Promotions_Category FOREIGN KEY (CategoryId) REFERENCES dbo.ToolCategories(CategoryId)
);
GO

-- FK added here because Promotions is defined after Bookings
ALTER TABLE dbo.Bookings
    ADD CONSTRAINT FK_Bookings_Promotion FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(PromotionId);
GO

CREATE TABLE dbo.Broadcasts (
    BroadcastId         INT IDENTITY(1,1) PRIMARY KEY,
    ZoneId              INT NULL,                          -- NULL = all zones
    Title               NVARCHAR(200) NULL,
    Message             NVARCHAR(MAX) NOT NULL,
    Priority            NVARCHAR(20) NOT NULL DEFAULT 'Normal'
                        CHECK (Priority IN ('Normal','High','Emergency')),
    RecipientCount      INT NOT NULL DEFAULT 0,
    SentByUserId        INT NOT NULL,
    SentAt              DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Broadcasts_Zone FOREIGN KEY (ZoneId)       REFERENCES dbo.Zones(ZoneId),
    CONSTRAINT FK_Broadcasts_Sender FOREIGN KEY (SentByUserId) REFERENCES dbo.Users(UserId)
);
GO

/* ---------------------------------------------------------------------
   REFERRALS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Referrals (
    ReferralId          INT IDENTITY(1,1) PRIMARY KEY,
    ReferrerUserId      INT NOT NULL,
    ReferredUserId      INT NOT NULL,
    ReferralCode        NVARCHAR(50) NOT NULL,
    CreditAmount        DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Pending'
                        CHECK (Status IN ('Pending','Completed','Cancelled')),
    CompletedAt         DATETIME2 NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Referrals_Referrer FOREIGN KEY (ReferrerUserId)  REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Referrals_Referred FOREIGN KEY (ReferredUserId)  REFERENCES dbo.Users(UserId)
);
GO

/* ---------------------------------------------------------------------
   MAINTENANCE / TECHNICIANS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.TechnicianProfiles (
    UserId                  INT PRIMARY KEY,               -- 1:1 with Users (role=Technician)
    Specializations         NVARCHAR(MAX) NULL,            -- JSON array or comma list
    AssignedZoneIds         NVARCHAR(MAX) NULL,            -- JSON array of ZoneIds
    ToolsRepairedCount      INT NOT NULL DEFAULT 0,
    AvgRepairTimeDays       DECIMAL(5,2) NULL,
    SuccessRate             DECIMAL(5,2) NULL,
    Rating                  NVARCHAR(20) NULL
                            CHECK (Rating IN ('Excellent','VeryGood','Good','Fair')),
    OpenTasksCount          INT NOT NULL DEFAULT 0,
    Bio                     NVARCHAR(MAX) NULL,
    CONSTRAINT FK_TechProfiles_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE TABLE dbo.MaintenanceRecords (
    MaintenanceId       INT IDENTITY(1,1) PRIMARY KEY,
    ToolId              INT NOT NULL,
    TechnicianUserId    INT NULL,
    Type                NVARCHAR(20) NOT NULL
                        CHECK (Type IN ('Inspection','Repair','Cleaning','Battery','External')),
    Description         NVARCHAR(MAX) NULL,
    Status              NVARCHAR(30) NOT NULL DEFAULT 'Scheduled'
                        CHECK (Status IN ('Scheduled','InProgress','Passed','Completed',
                                          'NeedsAttention','Failed','RetestNeeded')),
    Notes               NVARCHAR(MAX) NULL,
    DurationHours       DECIMAL(6,2) NULL,
    ScheduledAt         DATETIME2 NULL,
    CompletedAt         DATETIME2 NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Maintenance_Tool  FOREIGN KEY (ToolId)           REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_Maintenance_Tech  FOREIGN KEY (TechnicianUserId) REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_Maintenance_Tool ON dbo.MaintenanceRecords(ToolId);
GO

CREATE TABLE dbo.ExternalRepairs (
    ExternalRepairId        INT IDENTITY(1,1) PRIMARY KEY,
    ToolId                  INT NOT NULL,
    MaintenanceRecordId     INT NULL,
    RepairCenterName        NVARCHAR(200) NOT NULL,
    ProblemDescription      NVARCHAR(MAX) NULL,
    SentAt                  DATE NULL,
    ExpectedReturnAt        DATE NULL,
    ActualReturnAt          DATE NULL,
    Cost                    DECIMAL(18,2) NULL,
    InvoiceUrl              NVARCHAR(500) NULL,
    Status                  NVARCHAR(20) NOT NULL DEFAULT 'Inspecting'
                            CHECK (Status IN ('Inspecting','WaitingParts','Completed','Cancelled')),
    CreatedAt               DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ExtRepairs_Tool FOREIGN KEY (ToolId) REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_ExtRepairs_Maint FOREIGN KEY (MaintenanceRecordId) REFERENCES dbo.MaintenanceRecords(MaintenanceId)
);
GO

CREATE TABLE dbo.Disposals (
    DisposalId              INT IDENTITY(1,1) PRIMARY KEY,
    ToolId                  INT NOT NULL,
    Reason                  NVARCHAR(30) NOT NULL
                            CHECK (Reason IN ('IrreparableDamage','SafetyRisk','EndOfLife','OwnerRequest')),
    ReasonDescription       NVARCHAR(MAX) NULL,
    RecycleCenter           NVARCHAR(200) NULL,
    Photos                  NVARCHAR(MAX) NULL,            -- JSON array
    CancelFutureBookings    BIT NOT NULL DEFAULT 1,
    DisposedAt              DATE NULL,
    Status                  NVARCHAR(20) NOT NULL DEFAULT 'Pending'
                            CHECK (Status IN ('Pending','PendingPickup','Recycled','Cancelled')),
    ApprovedByUserId        INT NULL,
    CreatedAt               DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Disposals_Tool FOREIGN KEY (ToolId) REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_Disposals_Approver FOREIGN KEY (ApprovedByUserId) REFERENCES dbo.Users(UserId)
);
GO

/* ---------------------------------------------------------------------
   OPERATIONS — librarian tasks, audit log, reports, settings
   ------------------------------------------------------------------- */

CREATE TABLE dbo.LibrarianTasks (
    TaskId              INT IDENTITY(1,1) PRIMARY KEY,
    LibrarianUserId     INT NOT NULL,
    ZoneId              INT NULL,
    Type                NVARCHAR(40) NOT NULL
                        CHECK (Type IN ('QRHandoverVerification','DisputeReview','AuditApproval',
                                        'LateReturnFollowUp','InsuranceClaimProcessing',
                                        'KYCReview','Broadcast','Other')),
    RelatedEntityType   NVARCHAR(30) NULL,              -- Booking, Dispute, Audit...
    RelatedEntityId     NVARCHAR(50) NULL,
    Title               NVARCHAR(200) NOT NULL,
    Description         NVARCHAR(MAX) NULL,
    DueAt               DATETIME2 NULL,
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Pending'
                        CHECK (Status IN ('Pending','InProgress','Completed','Cancelled')),
    Priority            NVARCHAR(20) NOT NULL DEFAULT 'Normal'
                        CHECK (Priority IN ('Low','Normal','High','Emergency')),
    CompletedAt         DATETIME2 NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Tasks_Librarian FOREIGN KEY (LibrarianUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Tasks_Zone      FOREIGN KEY (ZoneId)          REFERENCES dbo.Zones(ZoneId)
);
GO
CREATE INDEX IX_Tasks_Librarian ON dbo.LibrarianTasks(LibrarianUserId, Status);
GO

CREATE TABLE dbo.AuditLogs (
    AuditLogId      BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NULL,                            -- NULL = system
    Action          NVARCHAR(30) NOT NULL
                    CHECK (Action IN ('Create','Update','Delete','Approve','Reject',
                                      'ConfigChange','Login','KYCReview','Payment',
                                      'DisputeResolve','Suspension','Other')),
    EntityType      NVARCHAR(50) NOT NULL,               -- User, Tool, Booking, Dispute...
    EntityId        NVARCHAR(50) NULL,
    Details         NVARCHAR(MAX) NULL,                  -- JSON payload
    IpAddress       NVARCHAR(50) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_AuditLogs_Entity ON dbo.AuditLogs(EntityType, EntityId);
CREATE INDEX IX_AuditLogs_Created ON dbo.AuditLogs(CreatedAt);
GO

/* Monthly revenue aggregation used by reports page */
CREATE TABLE dbo.RevenueReports (
    RevenueReportId     INT IDENTITY(1,1) PRIMARY KEY,
    PeriodMonth         DATE NOT NULL,                  -- first day of month
    Source              NVARCHAR(50) NOT NULL
                        CHECK (Source IN ('RentalCommissions','MembershipSubscriptions',
                                          'LateFines','DepositForfeits','BoostCredits',
                                          'InsurancePayouts','RefundsIssued')),
    TransactionCount    INT NOT NULL DEFAULT 0,
    Amount              DECIMAL(18,2) NOT NULL DEFAULT 0,
    SharePercentage     DECIMAL(5,2) NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_RevenueReports_Period_Source UNIQUE (PeriodMonth, Source)
);
GO

/* Singleton platform settings row */
CREATE TABLE dbo.PlatformSettings (
    SettingsId                          INT PRIMARY KEY DEFAULT 1 CHECK (SettingsId = 1),
    PlatformCommissionPercentage        DECIMAL(5,2) NOT NULL DEFAULT 18,
    InsurancePercentage                 DECIMAL(5,2) NOT NULL DEFAULT 5,
    DefaultDepositPercentage            DECIMAL(5,2) NOT NULL DEFAULT 20,
    DefaultMaxBorrowDays                INT NOT NULL DEFAULT 7,
    DefaultCleaningBufferHours          INT NOT NULL DEFAULT 10,
    LateReturnGraceHours                INT NOT NULL DEFAULT 2,
    LateReturnPenaltyPerDay             DECIMAL(18,2) NOT NULL DEFAULT 50,
    BookingAutoApproveEnabled           BIT NOT NULL DEFAULT 0,
    EmailBookingConfirmationsEnabled    BIT NOT NULL DEFAULT 1,
    SmsLateReturnAlertsEnabled          BIT NOT NULL DEFAULT 1,
    PushNotificationsEnabled            BIT NOT NULL DEFAULT 1,
    WeeklyMaintenanceRemindersEnabled   BIT NOT NULL DEFAULT 1,
    PlatformPaused                      BIT NOT NULL DEFAULT 0,
    UpdatedAt                           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedByUserId                     INT NULL,
    CONSTRAINT FK_Settings_Updater FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.Users(UserId)
);
GO
INSERT INTO dbo.PlatformSettings (SettingsId) VALUES (1);
GO

/* ---------------------------------------------------------------------
   MESSAGING  (masked lender-borrower chat)
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Conversations (
    ConversationId      UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BookingId           INT NULL,
    User1Id             INT NOT NULL,               -- lower UserId for canonical pair
    User2Id             INT NOT NULL,               -- higher UserId
    ToolId              INT NULL,
    Subject             NVARCHAR(200) NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    LastMessageAt       DATETIME2 NULL,
    CONSTRAINT FK_Conversations_Booking FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_Conversations_User1   FOREIGN KEY (User1Id)   REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Conversations_User2   FOREIGN KEY (User2Id)   REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Conversations_Tool    FOREIGN KEY (ToolId)    REFERENCES dbo.Tools(ToolId)
);
GO
-- one conversation per user-pair per booking; NULL booking handled by filtered index
CREATE UNIQUE INDEX UX_Conversations_PairNoBooking
    ON dbo.Conversations(User1Id, User2Id) WHERE BookingId IS NULL;
CREATE UNIQUE INDEX UX_Conversations_PairBooking
    ON dbo.Conversations(User1Id, User2Id, BookingId) WHERE BookingId IS NOT NULL;
CREATE INDEX IX_Conversations_User1 ON dbo.Conversations(User1Id);
CREATE INDEX IX_Conversations_User2 ON dbo.Conversations(User2Id);
GO

CREATE TABLE dbo.Messages (
    MessageId           BIGINT IDENTITY(1,1) PRIMARY KEY,
    ConversationId      UNIQUEIDENTIFIER NOT NULL,
    SenderUserId        INT NOT NULL,
    Body                NVARCHAR(MAX) NOT NULL,      -- masked/encrypted at app layer
    IsRead              BIT NOT NULL DEFAULT 0,
    ReadAt              DATETIME2 NULL,
    SentAt              DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Messages_Conversation FOREIGN KEY (ConversationId) REFERENCES dbo.Conversations(ConversationId) ON DELETE CASCADE,
    CONSTRAINT FK_Messages_Sender       FOREIGN KEY (SenderUserId)   REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_Messages_Conversation ON dbo.Messages(ConversationId, SentAt);
GO

/* ---------------------------------------------------------------------
   NOTIFICATIONS
   ------------------------------------------------------------------- */

CREATE TABLE dbo.Notifications (
    NotificationId      BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId              INT NOT NULL,
    Type                NVARCHAR(20) NOT NULL
                        CHECK (Type IN ('Booking','Payment','Maintenance','Dispute','System','Promotion')),
    Title               NVARCHAR(200) NOT NULL,
    Message             NVARCHAR(MAX) NULL,
    Priority            NVARCHAR(20) NOT NULL DEFAULT 'Normal'
                        CHECK (Priority IN ('Low','Normal','High','Emergency')),
    ActionUrl           NVARCHAR(500) NULL,
    IsRead              BIT NOT NULL DEFAULT 0,
    ReadAt              DATETIME2 NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO
CREATE INDEX IX_Notifications_User ON dbo.Notifications(UserId, IsRead, CreatedAt);
GO

/* ---------------------------------------------------------------------
   AVAILABILITY CALENDAR  (per-tool day status)
   ------------------------------------------------------------------- */

CREATE TABLE dbo.ToolAvailability (
    AvailabilityId      BIGINT IDENTITY(1,1) PRIMARY KEY,
    ToolId              INT NOT NULL,
    Date                DATE NOT NULL,
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Available'
                        CHECK (Status IN ('Available','Booked','CleaningBuffer','Unavailable','Maintenance')),
    BookingId           INT NULL,
    CONSTRAINT FK_Availability_Tool    FOREIGN KEY (ToolId)    REFERENCES dbo.Tools(ToolId),
    CONSTRAINT FK_Availability_Booking FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT UQ_ToolAvailability UNIQUE (ToolId, Date)
);
GO
CREATE INDEX IX_Availability_Date ON dbo.ToolAvailability(Date, Status);
GO

/* ---------------------------------------------------------------------
   SEED DATA — roles, categories, zones, demo users
   ------------------------------------------------------------------- */

INSERT INTO dbo.Roles (Name) VALUES ('Member'), ('Librarian'), ('Technician'), ('Admin');
GO

INSERT INTO dbo.ToolCategories (Name, Icon) VALUES
    ('Power Tools',    'fas fa-bolt'),
    ('Hand Tools',     'fas fa-screwdriver-wrench'),
    ('Garden',         'fas fa-seedling'),
    ('3D Printing',    'fas fa-cube'),
    ('Woodworking',    'fas fa-hammer');
GO

INSERT INTO dbo.Zones (Name) VALUES
    ('Nasr City'), ('Maadi'), ('Heliopolis'), ('Dokki'), ('Zamalek'),
    ('New Cairo'), ('6th of October'), ('Shubra');
GO

-- Demo users (replace PasswordHash with real ASP.NET Identity hashes)
INSERT INTO dbo.Users (RoleId, FullName, Email, Phone, PasswordHash, KycStatus, Tier, ZoneId, TrustScore, RatingAverage, ReferralCode)
SELECT r.RoleId, u.FullName, u.Email, u.Phone, u.Hash, 'Verified', u.Tier, z.ZoneId, u.Trust, u.Rating, u.Ref
FROM (VALUES
    ('Admin',       'System Admin',    'admin@toolshare.local',    '01000000001', 'HASH_ADMIN',       'Staff',  'Nasr City',       50, 5.00, 'ADMIN10'),
    ('Librarian',   'Mona Adel',       'librarian@toolshare.local','01000000002', 'HASH_LIBRARIAN',   'Staff',  'Nasr City',       20, 4.80, 'LIB10'),
    ('Technician',  'Mahmoud Samir',   'tech@toolshare.local',     '01000000003', 'HASH_TECH',        'Staff',  'Heliopolis',      10, 4.90, 'TECH10'),
    ('Member',      'Ahmed Hassan',    'member@toolshare.local',   '01000000004', 'HASH_MEMBER',      'Member', 'Maadi',           10, 4.60, 'AHMED10'),
    ('Member',      'Sara Mostafa',    'sara@toolshare.local',     '01000000005', 'HASH_SARA',        'Pro',    'Zamalek',         20, 4.70, 'SARA10'),
    ('Member',      'Monika Adel',     'monika@toolshare.local',   '01000000006', 'HASH_MONIKA',      'Pro',    'Nasr City',       20, 4.90, 'MONIKA10')
) AS u(RoleName, FullName, Email, Phone, Hash, Tier, ZoneName, Trust, Rating, Ref)
JOIN dbo.Roles r ON r.Name = u.RoleName
JOIN dbo.Zones z ON z.Name = u.ZoneName;
GO

-- Demo tools matching the prototype listings
INSERT INTO dbo.Tools
    (OwnerUserId, CategoryId, ZoneId, Name, Brand, Description, Condition, Status,
     PricePerHour, PricePerDay, PricePerWeek, DepositAmount, InsurancePercentage,
     MaxBorrowDays, CleaningBufferHours, RatingAverage, ReviewCount, TotalRentals, IsHighValue)
SELECT
    (SELECT UserId FROM dbo.Users WHERE Email = 'monika@toolshare.local'),
    c.CategoryId, z.ZoneId, t.Name, t.Brand, t.Descr, t.Cond, 'Available',
    t.Ph, t.Pd, t.Pw, t.Dep, 5.00, 7, 10, t.Rating, t.Reviews, t.Rentals, t.HighVal
FROM (VALUES
    ('Power Tools',  'Nasr City', 'Cordless Drill',   'Bosch',    '18V brushless drill',            'Excellent', 10.00, 75.00,  400.00, 500.00, 4.9, 34, 58, 0),
    ('Power Tools',  'Nasr City', 'Angle Grinder',    'Makita',   '4.5 inch angle grinder',         'Good',      15.00, 90.00,  480.00, 400.00, 4.7, 21, 33, 0),
    ('3D Printing',  'Zamalek',   '3D Printer',       'Prusa',    'MK3S+ resin printer',            'Excellent', 50.00, 300.00, 1500.00, 2000.00, 4.8, 12, 19, 1),
    ('Garden',       'Maadi',     'Garden Shears',    'Fiskars',  'Bypass pruning shears',          'Good',       5.00, 30.00,  150.00,  50.00, 4.5, 18, 44, 0),
    ('Hand Tools',   'Heliopolis', 'Tool Kit',        'Stanley',  '65-piece home tool set',        'Good',       8.00, 50.00,  250.00, 200.00, 4.6, 27, 71, 0)
) AS t(CategoryName, ZoneName, Name, Brand, Descr, Cond, Ph, Pd, Pw, Dep, Rating, Reviews, Rentals, HighVal)
JOIN dbo.ToolCategories c ON c.Name = t.CategoryName
JOIN dbo.Zones z ON z.Name = t.ZoneName;
GO
