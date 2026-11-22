# P2P Tool Library

A peer-to-peer tool-rental platform built with **ASP.NET Core MVC** (.NET 10) and **SQL Server**. Members list tools for rent and borrow from each other; librarians, technicians, and admins run the platform through role-based dashboards.

## Features

- **Booking lifecycle** — request → pay (wallet) → confirm → QR-verified pickup/return handover → complete, with escrow deposit release
- **Wallet & payments** — balance top-up, rental + platform fee + insurance + deposit split, refunds and clawbacks
- **Disputes** — damage/payment reports routed to librarians with deposit-at-risk resolution (full/partial/split refunds)
- **Handover verification** — QR codes + checklist + mandatory photos, verified by library staff
- **Maintenance module** — health board, service records, external repairs with invoices, disposals, technician metrics
- **Insurance claims, late-return penalties, KYC, suspensions, broadcasts, promotions, referrals, ratings, inventory audits, revenue reports**
- **SignalR chat**, notifications, background jobs (late returns, maintenance reminders)

## Solution structure

```
├── Business Logic Layer (BLL)/   Interfaces + Services + Jobs (all business logic)
├── Data Access Layer (DAL)/      EF Core: entities, enums, configurations, migrations, view models
├── Presentation Layer (PL)/      5 controllers (Home, Member, Library, Maintenance, Admin) + Razor views + SignalR hub
├── tests/Core.Tests/             xUnit tests (EF Core InMemory)
├── wwwroot/                      Static assets (css/js/lib) + user uploads
├── database/schema.sql           Database source of truth
└── docs/ROADMAP.md               Feature roadmap
```

Controllers are thin — all data access and business rules live in BLL services.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local instance on `localhost:1433`)

## Run

```bash
# 1. Create the database (SQL Server)
#    Run database/schema.sql in SSMS / sqlcmd against your instance,
#    or let EF create it: dotnet ef database update

# 2. Update the connection string in appsettings.json if needed
#    "Server=localhost;Database=P2PToolLibrary;Trusted_Connection=True;TrustServerCertificate=True"

# 3. Run
dotnet run --urls http://localhost:5080
```

Or double-click `start.bat` on Windows.

## Test

```bash
dotnet test
```

## Test accounts (dev seed)

| Role        | Email                        | Password      |
|-------------|------------------------------|---------------|
| Member      | test@example.com             | NewPass@123   |
| Librarian   | librarian@toolshare.local    | Lib@12345     |
| Technician  | tech@toolshare.local         | Tech@12345    |
| Admin       | admin@toolshare.local        | Lib@12345     |

Each role lands on its own dashboard after login (`/Member`, `/Library`, `/Maintenance`, `/Admin`).

## Regression check

`regression.ps1` hits all ~70 routes as guest/member/librarian/tech/admin and reports status codes:

```powershell
.\regression.ps1
```
