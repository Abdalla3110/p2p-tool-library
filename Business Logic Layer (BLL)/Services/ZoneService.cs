using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ZoneService : IZoneService
{
    private readonly AppDbContext _db;
    public ZoneService(AppDbContext db) => _db = db;

    public async Task<List<Zone>> GetAllAsync() =>
        await _db.Zones.OrderBy(z => z.Name).ToListAsync();

    public async Task<Zone?> GetByIdAsync(int id) =>
        await _db.Zones.FindAsync(id);

    public async Task<List<ZoneOverviewRow>> GetOverviewAsync()
    {
        var zones = await _db.Zones.Include(z => z.LibrarianUser)
            .OrderBy(z => z.Name).ToListAsync();
        var zoneIds = zones.Select(z => z.ZoneId).ToList();
        var memberCounts = await _db.Users
            .Where(u => u.ZoneId != null && zoneIds.Contains(u.ZoneId!.Value))
            .GroupBy(u => u.ZoneId!.Value)
            .Select(g => new { g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Cnt);
        var toolCounts = await _db.Tools.Where(t => zoneIds.Contains(t.ZoneId))
            .GroupBy(t => t.ZoneId)
            .Select(g => new { g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Cnt);
        return zones.Select(z => new ZoneOverviewRow
        {
            Zone = z,
            MemberCount = memberCounts.GetValueOrDefault(z.ZoneId),
            ToolCount = toolCounts.GetValueOrDefault(z.ZoneId),
        }).ToList();
    }
}
