using DAL.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IToolDocumentService
{
    Task<ToolPhoto> AddPhotoAsync(int toolId, string url, DocumentTypeEnum? documentType, DateOnly? warrantyExpiry);
    Task<List<ToolPhoto>> GetForToolAsync(int toolId);
    Task DeleteAsync(int photoId, int ownerUserId);
    Task<ToolPhoto?> SaveUploadedPhotoAsync(int toolId, IFormFile file, int sortOrder, bool isPrimary);
}
