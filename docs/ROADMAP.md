# P2P Tool Library — Remaining Work Plan

Current state: all 4 role modules are functional on real SQL Server data.
This document tracks everything still missing, grouped by priority and effort.

---

## Phase A — Member-facing gaps (completes the rental lifecycle)

### A1. Member dispute submission
- **What**: member opens a dispute from a booking (damage, no-show, misdescription, payment issue).
- **DB**: `Disputes` (Type, Description, BookingId, ReportedByUserId, AgainstUserId/AgainstType, DepositAtRisk).
- **UI**: "Open dispute" button on `/Bookings/Details` (Active/Completed bookings) + `/Member/Disputes` list page.
- **Logic**: create Dispute (Status=Open) → set Booking.Status="Disputed" → freeze deposit release → notify assigned librarian (Zone.LibrarianUserId) + create LibrarianTask.
- **Files**: `MemberController` or new `DisputesController`, `Views/Member/Disputes.cshtml`, `DisputeForm.cshtml`.

### A2. Member insurance claim submission
- **What**: member files claim for tool damage/loss during rental.
- **DB**: `InsuranceClaims` (ClaimantUserId, ToolId, BookingId, ToolValue, IncidentDate, Description, EvidenceUrls, Status=Open).
- **UI**: claim form from booking detail + "My claims" list under Member.
- **Logic**: insert claim → notify librarian → link to related DamageReport if exists.
- **Files**: `MemberController.Claims` actions or `ClaimsController`, views.

### A3. Damage report → auto-dispute link
- **What**: when a damage report is "serious" (or always), auto-create a Dispute and set `DamageReport.DisputeId` + status `LinkedToDispute`.
- **Logic**: in `BookingsController.ReportDamage` → create Dispute row → update DamageReport.
- **Small change**, high value.

### A4. Profile editing
- **What**: edit FullName/Phone/Bio/Avatar/Zone + notification preferences toggles.
- **DB**: `Users` fields already exist (Bio, AvatarUrl, EmailNotificationsEnabled, SmsNotificationsEnabled, PushNotificationsEnabled).
- **UI**: `/Member/Profile` GET/POST + avatar upload (reuse uploads folder).
- **Files**: `MemberController.Profile`, `Views/Member/Profile.cshtml`.

### A5. Password change (logged-in)
- **What**: change password with current-password verification.
- **Logic**: verify via `PasswordHasher.Verify` → rehash → update.
- **UI**: section in Profile page.

### A6. Ratings page
- **What**: "Ratings I received" + "Ratings I gave" list.
- **DB**: `Ratings` (RaterUserId, RatedUserId, Rating, Comment, BookingId).
- **UI**: `/Member/Ratings` + link from dashboard.

### A7. Referral page
- **What**: show my referral code, share link, list referrals + bonuses earned.
- **DB**: `Referrals` (ReferrerUserId, ReferredUserId, Status, BonusAmount?), `Users.ReferralCode`.
- **Logic**: registration accepts referral code → create Referral row → on first completed booking, credit bonus to referrer wallet.
- **UI**: `/Member/Referral`.

### A8. Booking calendar UI
- **What**: month calendar on tool detail showing booked/unavailable days.
- **DB**: `ToolAvailability` (already populated by bookings).
- **UI**: `/Tools/Availability/{id}` or embedded calendar component on Details page; JS month grid colored by status.
- **Static JS exists?** — need small custom JS or reuse prototype `booking-calendar.html` logic.

### A9. Checkout page
- **What**: dedicated `/Bookings/Checkout/{id}` — order summary, wallet balance, payment method (wallet only for now), terms checkbox → POST Pay.
- **Why**: current inline Pay button works but isn't a real checkout UX; also place to apply promo codes (see B1).
- **Files**: `BookingsController.Checkout` GET + reuse Pay POST.

---

## Phase B — Business rules & automation

### B1. Promotions / discount codes
- **What**: admin creates promo codes; member applies at checkout.
- **DB**: `Promotions` (Code, Type=Percent|Fixed, Value, MaxUses, UsedCount, ValidFrom/To, MinOrderAmount, IsActive).
- **Logic**: validate at checkout → store `DiscountAmount` on Booking (needs column or compute) → reduce FinalAmountDue.
- **Schema change**: add `Bookings.DiscountAmount DECIMAL` + maybe `PromotionId`.
- **UI**: admin CRUD under `/Admin/Promotions` + promo input on checkout.

### B2. Referral bonus logic
- Covered in A7 — bonus triggers on referred user's first completed booking.

### B3. Late returns detection (background job)
- **What**: `BackgroundService` scanning every N minutes: bookings `Active`/`Confirmed` where `ReturnDate + grace < now` → create `LateReturns` row + penalty transaction + notify borrower/lender + escalate status (Warning → FineApplied → AccountFrozen per `PlatformSettings`).
- **Files**: `Core/Jobs/LateReturnJob.cs` registered in `Program.cs` (`AddHostedService`).
- **Config**: interval + grace hours from `PlatformSettings`.

### B4. Maintenance reminders job
- **What**: weekly scan — tools with `LastServiceAt` older than threshold or `BatteryHealthPercentage < 30` → notify owner + create LibrarianTask for technician.
- **Files**: `Core/Jobs/MaintenanceReminderJob.cs`.

### B5. Deposit release partial/split calculation
- **What**: librarian dispute resolution with exact amounts: borrower refund + lender payout + platform fee, all recorded as `Transactions`.
- **Current**: `ResolveDispute` sets `DepositStatus` label only — no money movement.
- **Logic**: compute split (e.g., 60% lender / 40% borrower) → credit/debit wallets → `Transactions` rows (Refund/Credit/Forfeit) → update deposit.
- **UI**: resolution form already has `refundAmount` — extend with lender share %.

### B6. Cancellation fees
- **What**: cancelling a Confirmed/Active booking incurs fee (e.g., % of rental) → transaction + deposit partial forfeit.
- **Logic**: in `BookingsController.Cancel` — if status Confirmed → charge `LateReturnPenaltyPerDay`-like fee or % from settings; refund rest of deposit.
- **Decision needed**: fee policy (flat % vs per-day).

### B7. Booking reschedule
- **What**: borrower/lender proposes new dates → other party accepts → update booking + availability slots.
- **DB**: reuse `Conversations`/`Messages` or new `BookingChangeRequests`? Simplest: message-based proposal + admin/librarian confirms, or direct reschedule with notification.
- **Scope**: medium — could defer.

### B8. Damage evidence photos
- Extend `ReportDamage` + claim forms with file upload → `DamageReports.PhotoUrl`, `InsuranceClaims.EvidenceUrls` (JSON array or separate table).
- Reuse `wwwroot/uploads/damage/`.

---

## Phase C — Infrastructure & production hardening

### C1. Password reset
- **Needs**: SMTP/email provider. Options:
  - Dev: log reset link to console / store token shown in UI (NOT production-safe).
  - Prod: SendGrid/SES/SMTP config in `appsettings`.
- **Schema**: `Users.PasswordResetToken`, `PasswordResetExpiresAt` columns (or `PasswordResetTokens` table).
- **UI**: `/Auth/ForgotPassword` + `/Auth/ResetPassword/{token}`.

### C2. Email verification
- `Users.EmailVerifiedAt` column + verification token flow (same infra as C1).
- Gate: booking requires verified email (or just warn).

### C3. Uploads for KYC / invoices / evidence
- KYC: `Member/Profile` or dedicated `/Member/Kyc` — national ID + selfie upload → `Users.KycIdPhotoUrl`, `KycSelfiePhotoUrl` → status Pending → librarian reviews (exists).
- Repair invoices: `Maintenance/RepairForm` — `InvoiceUrl` upload.
- Damage/claim evidence: B8.

### C3b. Image pipeline
- Currently raw files served from wwwroot — for production: resize/thumbnail (ImageSharp), virus scan, CDN.

### C4. SignalR chat
- `ChatHub` (join conversation group, broadcast messages) + JS client on Conversation page + unread badge live update.
- Replaces manual refresh.

### C5. Rate limiting
- `AddRateLimiter` middleware: login (already has lockout — keep), register, booking create, message send.
- Policy: e.g., 10 req/min per IP for auth endpoints.

### C6. Security headers
- `UseSecurityHeaders`-style middleware or package: CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, HSTS (already in prod branch).
- Anti-forgery already on all POST forms.

### C7. HTTPS production config
- `launchSettings` dev cert; production needs real cert (IIS/Kestrel/nginx) — deployment concern.

### C8. Health checks & logging
- `/health` endpoint (`AddHealthChecks`) for SQL Server.
- Structured logging (Serilog) for production.

### C9. EF Migrations baseline
- Decide: keep `schema.sql` as source of truth OR migrate to EF migrations.
- If EF: baseline existing DB by inserting `__EFMigrationsHistory` row for `InitialCreate`.
- Script: `database/migrate.ps1` or docs.

---

## Phase D — Testing & quality

### D1. More unit tests
- `AuthService.RegisterAsync` (needs in-memory EF — `UseInMemoryDatabase` or SQLite in-memory).
- Booking price calculation — **extract to service first** (`Core/Services/BookingPricing.cs`) so it's testable.
- `PasswordHasher` ✅ done.

### D2. Integration tests
- WebApplicationFactory tests: login flow, booking conflict, role authorization.

### D3. Extract business logic from controllers
- Booking create/pay/cancel pricing+escrow logic → `Core/Services/BookingService` (testable, reusable).
- Dispute resolution money movement → `Core/Services/DisputeService`.

---

## Suggested execution order

| Sprint | Items | Why first |
|---|---|---|
| 1 | A1, A2, A3, A4, A5 | Member can't complete lifecycle without these |
| 2 | B5, B6, B3, B4 | Money correctness + automation |
| 3 | A8, A9, B1, B8 | UX polish + promos |
| 4 | A6, A7, B7 | Engagement features |
| 5 | C1–C3 | Auth hardening (needs email decision) |
| 6 | C4–C9, D1–D3 | Production readiness |

## Open decisions needed

1. **Email provider** for password reset/verification — which service?
2. **Cancellation fee policy** — flat % or per-day?
3. **Promo code scope** — percent/fixed, stackable with referral bonus?
4. **Schema evolution** — keep `schema.sql` + manual ALTERs, or commit to EF migrations?
5. **Deployment target** — IIS on this XAMPP machine, or cloud (Azure/AWS)?
