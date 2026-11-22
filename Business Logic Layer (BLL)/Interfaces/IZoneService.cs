using DAL.Models.Entities;

namespace BLL.Interfaces;

public class ZoneOverviewRow
{
    public Zone Zone { get; set; } = null!;
    public int MemberCount { get; set; }
    public int ToolCount { get; set; }
}

public interface IZoneService
{
    Task<List<Zone>> GetAllAsync();
    Task<Zone?> GetByIdAsync(int id);
    Task<List<ZoneOverviewRow>> GetOverviewAsync();
}
