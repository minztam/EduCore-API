using EduCore.API.Data;
using EduCore.API.DTOs.Review;
using EduCore.API.Entities;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories.Implementations
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _response;
        private readonly INotificationRepository _notificationRepo;
        public ReviewRepository(EduCoreDbContext context, ResponseMessageResult response, INotificationRepository notificationRepo)
        {
            _context = context;
            _response = response;
            _notificationRepo = notificationRepo;
        }

        public async Task<ResponseMessageResult> GetAllForAdminAsync(ReviewAdminQuery query)
        {
            var reviews = _context.CourseReviews
                .AsNoTracking()
                .Include(x => x.Student)
                .Include(x => x.Course)
                .AsQueryable();

            if (query.CourseId.HasValue)
                reviews = reviews.Where(x => x.CourseId == query.CourseId.Value);

            if (query.Rating.HasValue)
                reviews = reviews.Where(x => x.Rating == query.Rating.Value);

            if (query.Approved.HasValue)
                reviews = reviews.Where(x => x.IsApproved == query.Approved.Value);

            if (query.Featured.HasValue)
                reviews = reviews.Where(x => x.IsFeatured == query.Featured.Value);

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.Trim().ToLower();

                reviews = reviews.Where(x =>
                    x.Comment.ToLower().Contains(keyword) ||
                    x.Student!.Name.ToLower().Contains(keyword) ||
                    x.Course!.Title.ToLower().Contains(keyword));
            }

            var totalItems = await reviews.CountAsync();

            var items = await reviews
                .OrderByDescending(x => x.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new
                {
                    x.Id,
                    StudentName = x.Student!.Name,
                    CourseTitle = x.Course!.Title,
                    x.Rating,
                    x.Comment,
                    x.CreatedAt,
                    x.IsApproved,
                    x.IsFeatured
                })
                .ToListAsync();

            var data = new
            {
                Items = items,
                query.Page,
                query.PageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize)
            };

            return _response.SetSuccess("Lấy danh sách đánh giá admin thành công", data);
        }
        public async Task<ResponseMessageResult> GetByCourseAsync(Guid courseId)
        {
            var courseExists = await _context.Courses
                .AnyAsync(x => x.Id == courseId);

            if (!courseExists)
                return _response.SetFail("Khóa học không tồn tại", 404);

            var data = await _context.CourseReviews
                .AsNoTracking()
                .Include(x => x.Student)
                .Where(x => x.CourseId == courseId && x.IsApproved)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ReviewItemResponse
                {
                    Id = x.Id,
                    StudentId = x.StudentId,
                    StudentName = x.Student!.Name,
                    AvatarUrl = x.Student.AvatarUrl,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            if (!data.Any())
                return _response.SetSuccess("Chưa có lượt đánh giá cho khóa học này", new List<ReviewItemResponse>());

            return _response.SetSuccess("Lấy danh sách đánh giá khóa học thành công",data);
        }
        public async Task<ResponseMessageResult> CreateAsync(Guid studentId, CreateReviewRequest req)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(x => x.Id == req.CourseId);

            if (course == null)
                return _response.SetFail("Khóa học không tồn tại", 404);

            var existed = await _context.CourseReviews
                .FirstOrDefaultAsync(x =>
                    x.CourseId == req.CourseId &&
                    x.StudentId == studentId);

            if (existed != null)
            {
                existed.Rating = req.Rating;
                existed.Comment = req.Comment;
                existed.CreatedAt = DateTime.UtcNow;
                existed.IsApproved = false;

                await _context.SaveChangesAsync();
                await SendNotify(studentId, course.Title);

                return _response.SetSuccess("Cập nhật đánh giá thành công",existed);
            }

            var review = new CourseReview
            {
                Id = Guid.NewGuid(),
                CourseId = req.CourseId,
                StudentId = studentId,
                Rating = req.Rating,
                Comment = req.Comment,
                CreatedAt = DateTime.UtcNow,
                IsApproved = false,
                IsFeatured = false
            };

            await _context.CourseReviews.AddAsync(review);
            await _context.SaveChangesAsync();

            await SendNotify(studentId, course.Title);
            return _response.SetSuccess("Gửi đánh giá thành công",review);
        }
        public async Task<ResponseMessageResult> ApproveAsync(Guid id)
        {
            var review = await _context.CourseReviews.FindAsync(id);

            if (review == null)
                return _response.SetFail("Đánh giá không tồn tại", 404);

            review.IsApproved = !review.IsApproved;
            await _context.SaveChangesAsync();

            return _response.SetSuccess(review.IsApproved ? "Duyệt đánh giá thành công": "Bỏ đánh giá thành công", review);
        }
        public async Task<ResponseMessageResult> GetFeaturedAsync()
        {
            var data = await _context.CourseReviews
                .Include(x => x.Student)
                .Where(x => x.IsApproved && x.IsFeatured)
                .OrderByDescending(x => x.CreatedAt)
                .Take(6)
                .Select(x => new ReviewItemResponse
                {
                    Id = x.Id,
                    StudentName = x.Student!.Name,
                    AvatarUrl = x.Student.AvatarUrl,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    IsFeatured = x.IsFeatured,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return _response.SetSuccess("Lấy danh sách đánh giá nổi bật thành công", data);
        }
        public async Task<ResponseMessageResult> UpdateAsync(Guid id, Guid studentId, UpdateReviewRequest req)
        {
            var review = await _context.CourseReviews
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);

            if (review == null)
                return _response.SetFail("Đánh giá không tồn tại", 404);

            review.Rating = req.Rating;
            review.Comment = req.Comment;
            review.CreatedAt = DateTime.UtcNow;
            review.IsApproved = false;

            await _context.SaveChangesAsync();
            await SendNotify(studentId, review.Course?.Title ?? "Khóa học");

            return _response.SetSuccess("Cập nhật đánh giá thành công", review);
        }
        public async Task<ResponseMessageResult> DeleteAsync(Guid id, Guid studentId)
        {
            var review = await _context.CourseReviews
                .FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);

            if (review == null)
                return _response.SetFail("Không tìm thấy đánh giá", 404);

            _context.CourseReviews.Remove(review);
            await _context.SaveChangesAsync();

            return _response.SetSuccess("Xóa đánh giá thành công");
        }
        public async Task<ResponseMessageResult> ToggleFeaturedAsync(Guid id)
        {
            var review = await _context.CourseReviews.FindAsync(id);

            if (review == null)
                return _response.SetFail("Đánh giá không tồn tại", 404);

            review.IsFeatured = !review.IsFeatured;

            await _context.SaveChangesAsync();

            return _response.SetSuccess(
                review.IsFeatured
                    ? "Đã bật đánh giá nổi bật"
                    : "Đã tắt đánh giá nổi bật",
                review
            );
        }
        public async Task<ResponseMessageResult> AdminDeleteAsync(Guid id)
        {
            var review = await _context.CourseReviews.FindAsync(id);

            if (review == null)
                return _response.SetFail("Không tìm thấy đánh giá", 404);

            _context.CourseReviews.Remove(review);
            await _context.SaveChangesAsync();

            return _response.SetSuccess("Admin đã xóa đánh giá thành công");
        }
        public async Task<ResponseMessageResult> GetReviewStatsAsync()
        {
            var total = await _context.CourseReviews.CountAsync();
            var pending = await _context.CourseReviews.CountAsync(x => !x.IsApproved);
            var featured = await _context.CourseReviews.CountAsync(x => x.IsFeatured);
            var avgRating = await _context.CourseReviews.AnyAsync()
                ? await _context.CourseReviews.AverageAsync(x => x.Rating)
                : 0;

            var data = new
            {
                TotalReviews = total,
                PendingReviews = pending,
                FeaturedReviews = featured,
                AverageRating = Math.Round(avgRating, 1)
            };

            return _response.SetSuccess("Lấy thống kê review thành công", data);
        }

            private async Task SendNotify(Guid studentId, string courseName)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    SenderId = studentId,
                    Content = $"đã gửi một đánh giá cho khóa học: {courseName}",
                    Type = "review",
                    RedirectUrl = "/admin/review",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepo.AddNotificationAsync(notification);
            }

    }
}
