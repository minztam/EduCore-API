using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface IChapterRepository
    {
        Task<ResponseMessageResult> GetAllAsync();
        Task<ResponseMessageResult> CreateAsync(string title, int order, Guid courseId);
        Task<ResponseMessageResult> UpdateAsync(Guid id, string? title, int? order, Guid? courseId);
        Task<ResponseMessageResult> DeleteAsync(Guid id);
        Task<ResponseMessageResult> GetByCourseIdAsync(Guid courseId);

    }
}
