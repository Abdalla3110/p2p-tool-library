using System;
using BLL.Jobs;
using BLL.Services;
using DAL.Data;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Views live inside "Presentation Layer (PL)/Views" instead of the default "/Views".
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Presentation Layer (PL)/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Presentation Layer (PL)/Views/Shared/{0}.cshtml");
});

builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

// rate limiting: strict on auth endpoints, relaxed global
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth", httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<BLL.Interfaces.IAuthService, AuthService>();
builder.Services.AddScoped<BLL.Interfaces.IDisputeService, DisputeService>();
builder.Services.AddScoped<BLL.Interfaces.IReservationService, ReservationService>();
builder.Services.AddScoped<BLL.Interfaces.IToolService, ToolService>();
builder.Services.AddScoped<BLL.Interfaces.IMessageService, MessageService>();
builder.Services.AddScoped<BLL.Interfaces.INotificationService, NotificationService>();
builder.Services.AddScoped<BLL.Interfaces.IUserService, UserService>();
builder.Services.AddScoped<BLL.Interfaces.IDepositService, DepositService>();
builder.Services.AddScoped<BLL.Interfaces.IPromoCodeService, PromoCodeService>();
builder.Services.AddScoped<BLL.Interfaces.ILibrarianService, LibrarianService>();
builder.Services.AddScoped<BLL.Interfaces.IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<BLL.Interfaces.IReportService, ReportService>();
builder.Services.AddScoped<BLL.Interfaces.IReviewService, ReviewService>();
builder.Services.AddScoped<BLL.Interfaces.IPenaltyService, PenaltyService>();
builder.Services.AddScoped<BLL.Interfaces.IAuditService, AuditService>();
builder.Services.AddScoped<BLL.Interfaces.ICategoryService, CategoryService>();
builder.Services.AddScoped<BLL.Interfaces.IToolDocumentService, ToolDocumentService>();
builder.Services.AddScoped<BLL.Interfaces.IZoneService, ZoneService>();
builder.Services.AddScoped<BLL.Interfaces.IDamageReportService, DamageReportService>();
builder.Services.AddScoped<BLL.Interfaces.IClaimService, ClaimService>();
builder.Services.AddScoped<BLL.Interfaces.IAdminService, AdminService>();

builder.Services.AddHostedService<LateReturnJob>();
builder.Services.AddHostedService<MaintenanceReminderJob>();

builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseRateLimiter();

// security headers on every response
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// health check endpoint
app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        var ok = await db.Database.CanConnectAsync();
        return ok ? Results.Ok(new { status = "healthy", db = "up" })
                  : Results.Json(new { status = "degraded", db = "down" }, statusCode: 503);
    }
    catch { return Results.Json(new { status = "unhealthy", db = "down" }, statusCode: 503); }
});

app.MapHub<PL.Hubs.ChatHub>("/hubs/chat");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
