using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface IEnrollmentRepository
    {
        Task<ResponseMessageResult> EnrollCourseAsync(Guid userId,Guid courseId,bool isPaid = false,string? paymentMethod = null);

        Task<ResponseMessageResult> CheckEnrollmentAsync(Guid userId, Guid courseId);
        Task<ResponseMessageResult> GetMyCoursesAsync(Guid userId);

        Task<ResponseMessageResult> GetEnrollmentByCourseAsync(Guid userId,Guid courseId);

        Task<ResponseMessageResult> UpdateLearningProgressAsync( Guid userId,Guid courseId,Guid lessonId);

        Task<ResponseMessageResult> UpdateLastAccessAsync(Guid userId,Guid courseId);
    }
}
