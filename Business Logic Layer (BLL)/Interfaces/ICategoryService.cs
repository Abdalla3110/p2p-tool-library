using DAL.Models.Entities;

namespace BLL.Interfaces;

public interface ICategoryService
{
    Task<List<ToolCategory>> GetAllAsync();
    Task<ToolCategory?> GetByIdAsync(int id);
}
