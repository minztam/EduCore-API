using EduCore.API.DTOs.Lesson;
using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface ILessonRepository
    {
        Task<ResponseMessageResult> GetByChapterIdAsync(Guid chapterId);
        Task<ResponseMessageResult> GetByIdAsync(Guid id);
        Task<ResponseMessageResult> CreateAsync(CreateLessonRequest model);
        Task<ResponseMessageResult> UpdateAsync(UpdateLessonRequest model);
        Task<ResponseMessageResult> DeleteAsync(Guid id);
    }
}
