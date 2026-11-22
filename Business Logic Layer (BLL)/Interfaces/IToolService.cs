using DAL.Models.Entities;
using DAL.Models.ViewModels;

namespace BLL.Interfaces;

public interface IToolService
{
    Task<Tool?> GetToolForBrowseAsync(int toolId);
    Task<Tool?> GetAnyToolWithCategoryAsync(int toolId);
    Task<int> GetOwnerUserIdAsync(int toolId);
    Task<List<Tool>> GetFeaturedAsync(int take = 8);
    Task<List<Tool>> GetPublicToolsByOwnerAsync(int ownerId);
    Task<IQueryable<Tool>> GetBrowseQueryAsync(BrowseToolsViewModel filter);
    Task<ToolDetailsViewModel?> GetDetailsViewModelAsync(int toolId, bool isOwner);
    Task<Dictionary<DateOnly, string>> GetMonthAvailabilityAsync(int toolId, int year, int month);

    // owner CRUD
    Task<List<Tool>> GetMyToolsAsync(int ownerId);
    Task<Tool?> GetOwnedToolAsync(int toolId, int ownerId);
    Task<Tool> SaveToolAsync(ToolFormViewModel model, int ownerId);

    // library approval
    Task<List<Tool>> GetToolsForApprovalAsync(string? status);
    Task ApproveToolAsync(int toolId);
    Task RejectToolAsync(int toolId);
}
