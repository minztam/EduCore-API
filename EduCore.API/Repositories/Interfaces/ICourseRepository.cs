using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<ResponseMessageResult> GetAllAsync();
        Task<ResponseMessageResult> CreateAsync(string instructorId, string title, string slug, string? desc, string content, IFormFile? thumb, IFormFile? preview, decimal price, string leve, string lang, Guid? categoryId);
        Task<ResponseMessageResult> UpdateAsync(Guid id, string? instructorId, string? title, string? slug, string? desc, string? content, IFormFile? thumb, IFormFile? preview, decimal? price, string? level, string? lang, Guid? categoryId);
        Task<ResponseMessageResult> TogglePublishAsync(Guid id);
        Task<ResponseMessageResult> ToggleHotAsync(Guid id);
        Task<ResponseMessageResult> SearchAsync(string key, int page = 1, int pageSize = 10);
        Task<ResponseMessageResult> GetByIdAsync(Guid id);
        Task<ResponseMessageResult> GetBySlugAsync(string slug);
        Task<ResponseMessageResult> DeleteAsync(Guid id);
        Task<ResponseMessageResult> FilterAsync(string? level, string? price, bool? isPublished, int page, int pageSize);
    }
}
