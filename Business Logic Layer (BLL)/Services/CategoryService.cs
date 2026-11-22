using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    public CategoryService(AppDbContext db) => _db = db;

    public async Task<List<ToolCategory>> GetAllAsync() =>
        await _db.ToolCategories.OrderBy(c => c.Name).ToListAsync();

    public async Task<ToolCategory?> GetByIdAsync(int id) =>
        await _db.ToolCategories.FindAsync(id);
}
