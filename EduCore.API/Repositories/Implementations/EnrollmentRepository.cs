using EduCore.API.Data;
using EduCore.API.Entities;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories.Implementations
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _response;
        public EnrollmentRepository(EduCoreDbContext context, ResponseMessageResult response)
        {
            _context = context;
            _response = response;
        }
        public async Task<ResponseMessageResult> EnrollCourseAsync(Guid userId, Guid courseId, bool isPaid = false, string? paymentMethod = null)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == userId);

                if (user == null)
                    return _response.SetFail("Người dùng không tồn tại");

                var course = await _context.Courses
                    .FirstOrDefaultAsync(x => x.Id == courseId);

                if (course == null)
                    return _response.SetFail("Khóa học không tồn tại");

                var existedEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.CourseId == courseId);

                if (existedEnrollment != null)
                    return _response.SetFail("Bạn đã tham gia khóa học này rồi");

                if (course.Price > 0 && !isPaid)
                {
                    return _response.SetFail(
                        "Khóa học này yêu cầu thanh toán trước khi tham gia");
                }

                var enrollment = new Enrollment
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CourseId = courseId,
                    EnrolledAt = DateTime.UtcNow,

                    ProgressPercent = 0,
                    CompletedLessons = 0,
                    IsCompleted = false,
                    CompletedAt = null,

                    IsPaid = course.Price == 0 ? true : isPaid,
                    PaidAt = (course.Price == 0 || isPaid)
                        ? DateTime.UtcNow
                        : null,

                    PaymentMethod = paymentMethod,
                    Status = "Active",
                    LastAccessedAt = DateTime.UtcNow
                };

                await _context.Enrollments.AddAsync(enrollment);
                await _context.SaveChangesAsync();

                return _response.SetSuccess(
                    course.Price == 0
                        ? "Tham gia khóa học miễn phí thành công"
                        : "Đăng ký khóa học thành công",
                    enrollment);
            }
            catch (Exception ex)
            {
                return _response.SetFail(
                    $"Lỗi khi tham gia khóa học: {ex.Message}");
            }
        }

        public async Task<ResponseMessageResult> CheckEnrollmentAsync(Guid userId, Guid courseId)
        {
            try
            {
                var isEnrolled = await _context.Enrollments.AnyAsync(x =>
                    x.UserId == userId &&
                    x.CourseId == courseId &&
                    x.Status == "Active");

                return _response.SetSuccess(
                    "Kiểm tra trạng thái tham gia thành công",
                    isEnrolled);
            }
            catch (Exception ex)
            {
                return _response.SetFail(
                    $"Lỗi kiểm tra enrollment: {ex.Message}");
            }
        }
        public async Task<ResponseMessageResult> GetMyCoursesAsync(Guid userId)
        {
            try
            {
                var data = await _context.Enrollments
                    .Where(x => x.UserId == userId && x.Status == "Active")
                    .Include(x => x.Course)
                    .OrderByDescending(x => x.LastAccessedAt)
                    .Select(x => new
                    {
                        x.Id,
                        x.ProgressPercent,
                        x.CompletedLessons,
                        x.LastAccessedAt,
                        x.IsCompleted,
                        Course = new
                        {
                            x.Course!.Id,
                            x.Course.Title,
                            x.Course.Slug,
                            x.Course.ThumbnailUrl,
                            x.Course.Level
                        }
                    })
                    .ToListAsync();

                return _response.SetSuccess("Lấy khóa học đã tham gia thành công", data);
            }
            catch (Exception ex)
            {
                return _response.SetFail($"Lỗi: {ex.Message}");
            }
        }
        public Task<ResponseMessageResult> GetEnrollmentByCourseAsync(Guid userId, Guid courseId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMessageResult> UpdateLastAccessAsync(Guid userId, Guid courseId)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseMessageResult> UpdateLearningProgressAsync(
            Guid userId,
            Guid courseId,
            Guid lessonId)
        {
            try
            {
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.CourseId == courseId &&
                        x.Status == "Active");

                if (enrollment == null)
                    return _response.SetFail("Bạn chưa tham gia khóa học");

                var completedIds = string.IsNullOrEmpty(enrollment.CompletedLessonIds)
                    ? new List<string>()
                    : enrollment.CompletedLessonIds.Split(',').ToList();

                if (completedIds.Contains(lessonId.ToString()))
                {
                    return _response.SetSuccess(
                        "Bài học đã hoàn thành trước đó",
                        enrollment);
                }

                completedIds.Add(lessonId.ToString());
                enrollment.CompletedLessonIds = string.Join(",", completedIds);

                var totalLessons = await _context.Lessons
                    .Where(x => x.Chapter.CourseId == courseId)
                    .CountAsync();

                enrollment.CompletedLessons = completedIds.Count;

                enrollment.ProgressPercent =
                    totalLessons == 0
                        ? 0
                        : Math.Round(
                            (double)enrollment.CompletedLessons / totalLessons * 100, 2);

                enrollment.LastAccessedAt = DateTime.UtcNow;

                if (enrollment.ProgressPercent >= 100)
                {
                    enrollment.ProgressPercent = 100;
                    enrollment.IsCompleted = true;
                    enrollment.CompletedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return _response.SetSuccess(
                    "Cập nhật tiến độ thành công",
                    enrollment);
            }
            catch (Exception ex)
            {
                return _response.SetFail($"Lỗi cập nhật tiến độ: {ex.Message}");
            }
        }
    }
}
