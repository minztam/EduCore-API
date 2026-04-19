using EduCore.API.DTOs.Review;
using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<ResponseMessageResult> GetAllForAdminAsync(ReviewAdminQuery query);
        Task<ResponseMessageResult> CreateAsync(Guid studentId, CreateReviewRequest req);
        Task<ResponseMessageResult> GetByCourseAsync(Guid courseId);
        Task<ResponseMessageResult> ApproveAsync(Guid id);
        Task<ResponseMessageResult> GetFeaturedAsync();
        Task<ResponseMessageResult> UpdateAsync(Guid id, Guid studentId, UpdateReviewRequest req);
        Task<ResponseMessageResult> DeleteAsync(Guid id, Guid studentId);
        Task<ResponseMessageResult> ToggleFeaturedAsync(Guid id);
        Task<ResponseMessageResult> AdminDeleteAsync(Guid id);
        Task<ResponseMessageResult> GetReviewStatsAsync();
    }
}
