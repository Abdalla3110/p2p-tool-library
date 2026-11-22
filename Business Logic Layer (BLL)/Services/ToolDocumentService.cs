using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ToolDocumentService : IToolDocumentService
{
    private readonly AppDbContext _db;
    public ToolDocumentService(AppDbContext db) => _db = db;

    public async Task<ToolPhoto> AddPhotoAsync(int toolId, string url, DocumentTypeEnum? documentType, DateOnly? warrantyExpiry)
    {
        var photo = new ToolPhoto
        {
            ToolId = toolId, Url = url, DocumentType = documentType,
            WarrantyExpiry = warrantyExpiry, SortOrder = 0,
        };
        _db.ToolPhotos.Add(photo);
        await _db.SaveChangesAsync();
        return photo;
    }

    public async Task<List<ToolPhoto>> GetForToolAsync(int toolId) =>
        await _db.ToolPhotos.Where(p => p.ToolId == toolId).OrderBy(p => p.SortOrder).ToListAsync();

    public async Task DeleteAsync(int photoId, int ownerUserId)
    {
        var photo = await _db.ToolPhotos.Include(p => p.Tool)
            .FirstOrDefaultAsync(p => p.PhotoId == photoId && p.Tool!.OwnerUserId == ownerUserId);
        if (photo is not null)
        {
            _db.ToolPhotos.Remove(photo);
            await _db.SaveChangesAsync();
        }
    }

    private static readonly HashSet<string> AllowedImageExts = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public async Task<ToolPhoto?> SaveUploadedPhotoAsync(int toolId, IFormFile file, int sortOrder, bool isPrimary)
    {
        if (file.Length == 0) return null;
        var ext = Path.GetExtension(file.FileName);
        if (!AllowedImageExts.Contains(ext)) return null;
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tools", toolId.ToString());
        Directory.CreateDirectory(dir);
        var fname = $"{Guid.NewGuid():N}{ext}";
        await using (var fs = System.IO.File.Create(Path.Combine(dir, fname)))
            await file.CopyToAsync(fs);
        var photo = new ToolPhoto
        {
            ToolId = toolId,
            Url = $"/uploads/tools/{toolId}/{fname}",
            IsPrimary = isPrimary,
            SortOrder = sortOrder,
        };
        _db.ToolPhotos.Add(photo);
        await _db.SaveChangesAsync();
        return photo;
    }
}
