using BLL.Services;
using DAL.Data;
using DAL.Models.Entities;
using DAL.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Core.Tests;

public class ServiceTests
{
    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedAsync(AppDbContext db)
    {
        db.Roles.AddRange(
            new Role { RoleId = 1, Name = "Member" },
            new Role { RoleId = 2, Name = "Librarian" });
        db.Zones.Add(new Zone { ZoneId = 1, Name = "Downtown" });
        db.Users.AddRange(
            new User { UserId = 1, FullName = "Lender", Email = "l@x.com", PasswordHash = "x", RoleId = 1, ReferralCode = "LEND1", WalletBalance = 1000, JoinedAt = DateTime.UtcNow, IsActive = true, KycStatus = KycStatusEnum.Verified, Tier = MembershipTierEnum.Pro },
            new User { UserId = 2, FullName = "Borrower", Email = "b@x.com", PasswordHash = "x", RoleId = 1, ReferralCode = "BORR1", WalletBalance = 500, JoinedAt = DateTime.UtcNow, IsActive = true, KycStatus = KycStatusEnum.Pending, Tier = MembershipTierEnum.Member },
            new User { UserId = 3, FullName = "Lib", Email = "lib@x.com", PasswordHash = "x", RoleId = 2, ReferralCode = "LIB01", WalletBalance = 0, JoinedAt = DateTime.UtcNow, IsActive = true, KycStatus = KycStatusEnum.Verified, Tier = MembershipTierEnum.Staff });
        db.Tools.Add(new Tool
        {
            ToolId = 1, OwnerUserId = 1, Name = "Drill", CategoryId = 1, ZoneId = 1,
            Status = ToolStatusEnum.Available, Condition = ToolConditionEnum.Good, PricePerDay = 50, DepositAmount = 200, MaxBorrowDays = 7,
            CleaningBufferHours = 10, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true,
        });
        db.Bookings.Add(new Booking
        {
            BookingId = 1, ToolId = 1, BorrowerUserId = 2, LenderUserId = 1,
            PickupDate = new DateOnly(2026, 12, 1), ReturnDate = new DateOnly(2026, 12, 3),
            RentalAmount = 100, InsuranceAmount = 5, PlatformFee = 18, DepositAmount = 200,
            TotalAmount = 323, FinalAmountDue = 123, Status = BookingStatusEnum.Active,
            DepositStatus = DepositStatusEnum.Held, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Register_CreatesUser_WithMemberRole()
    {
        using var db = NewDb();
        await SeedAsync(db);
        var svc = new AuthService(db);

        var err = await svc.RegisterAsync("New User", null, "new@x.com", "Pass@12345", null);
        Assert.Null(err);
        var user = await db.Users.FirstAsync(u => u.Email == "new@x.com");
        Assert.Equal(1, user.RoleId);
        Assert.True(BLL.Services.PasswordHasher.Verify("Pass@12345", user.PasswordHash));
        Assert.NotNull(user.EmailVerificationToken);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsError()
    {
        using var db = NewDb();
        await SeedAsync(db);
        var svc = new AuthService(db);
        var err = await svc.RegisterAsync("Dup", null, "l@x.com", "Pass@12345", null);
        Assert.NotNull(err);
    }

    [Fact]
    public async Task Register_WithReferral_CreatesPendingReferral()
    {
        using var db = NewDb();
        await SeedAsync(db);
        var svc = new AuthService(db);
        await svc.RegisterAsync("Referred", null, "ref@x.com", "Pass@12345", "LEND1");
        var r = await db.Referrals.FirstAsync();
        Assert.Equal(1, r.ReferrerUserId);
        Assert.Equal(ReferralStatusEnum.Pending, r.Status);
        Assert.Equal(100, r.CreditAmount);
    }

    [Fact]
    public async Task OpenDispute_MarksBookingDisputed_AndNotifies()
    {
        using var db = NewDb();
        await SeedAsync(db);
        var svc = new DisputeService(db);

        var d = await svc.OpenDisputeAsync(1, 2, DisputeTypeEnum.Damage, "Scratch found");

        Assert.Equal(DisputeStatusEnum.Open, d.Status);
        Assert.Equal("User", d.AgainstType);
        Assert.Equal(1, d.AgainstUserId); // lender is counterparty
        Assert.Equal(200, d.DepositAtRisk);

        var booking = await db.Bookings.FindAsync(1);
        Assert.Equal(BookingStatusEnum.Disputed, booking!.Status);

        // zone has no librarian → fallback to first active librarian (id 3)
        Assert.Equal(3, d.AssignedLibrarianUserId);
        Assert.True(await db.LibrarianTasks.AnyAsync(t => t.Type == "DisputeReview"));
        Assert.True(await db.Notifications.AnyAsync(n => n.UserId == 1)); // counterparty notified
    }

    [Fact]
    public async Task PasswordHasher_RejectsTamperedHash()
    {
        var hash = BLL.Services.PasswordHasher.Hash("secret");
        var tampered = hash[..^2] + "xx";
        Assert.False(BLL.Services.PasswordHasher.Verify("secret", tampered));
    }
}
