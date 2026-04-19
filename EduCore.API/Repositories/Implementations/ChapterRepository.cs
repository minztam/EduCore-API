using EduCore.API.Data;
using EduCore.API.Entities;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Cms;

namespace EduCore.API.Repositories.Implementations
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _respone;
        public ChapterRepository (EduCoreDbContext context, ResponseMessageResult respone)
        {
            _context = context;
            _respone = respone;
        }

        public async Task<ResponseMessageResult> GetAllAsync()
        {
            try
            {
                var result = await _context.Chapters
                    .OrderBy(x => x.Order)
                    .Select(x => new
                    {
                        x.Id,
                        x.Title,
                        x.Order,
                        x.CreatedAt,
                        x.UpdatedAt,
                        x.CourseId
                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (!result.Any())
                    return _respone.SetSuccess("Chưa có dữ liệu", result);

                return _respone.SetSuccess("Lấy danh sách thành công", result);
            }
            catch (Exception ex)
            {
                return _respone.SetFail($"Lỗi: {ex}");
            }
        }
        public async Task<ResponseMessageResult> CreateAsync(string title, int order, Guid courseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                    return _respone.SetFail("Tiêu đề không được để trống");

                if (order <= 0)
                    return _respone.SetFail("Vị trí phải lớn hơn 0");

                if (courseId == Guid.Empty)
                    return _respone.SetFail("Khóa học    không hợp lệ");

                var course = await _context.Courses
                    .FirstOrDefaultAsync(x => x.Id == courseId);

                if (course == null)
                    return _respone.SetFail("Khóa học không tồn tại");

                var isCheck = await _context.Chapters
                    .AnyAsync(x => x.CourseId == courseId && x.Order == order);

                if (isCheck)
                    return _respone.SetFail($"Vị trí số {order} đã tồn tại trong khóa học!");

                var chapter = new Chapter
                {
                    Id = Guid.NewGuid(),
                    Title = title.Trim(),
                    Order = order,
                    CourseId = courseId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Chapters.AddAsync(chapter);
                await _context.SaveChangesAsync();

                return _respone.SetSuccess("Tạo chapter thành công", new
                {
                    chapter.Id,
                    chapter.Title,
                    chapter.Order,
                    chapter.CreatedAt,
                    chapter.CourseId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return _respone.SetFail("Có lỗi xảy ra khi tạo chapter");
            }
        }
        public async Task<ResponseMessageResult> UpdateAsync(Guid id, string? title, int? order, Guid? courseId)
        {
            try
            {
                var chapter = await _context.Chapters
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (chapter == null)
                    return _respone.SetFail("Chương không tồn tại không tồn tại");

                if (title != null && string.IsNullOrWhiteSpace(title))
                    return _respone.SetFail("Tiêu đề không hợp lệ");

                if (order.HasValue && order <= 0)
                    return _respone.SetFail("Vị trí phải lớn hơn 0");

                if (courseId.HasValue && courseId == Guid.Empty)
                    return _respone.SetFail("Khóa học không hợp lệ");

                if (courseId.HasValue && courseId != chapter.CourseId)
                {
                    var course = await _context.Courses
                        .FirstOrDefaultAsync(x => x.Id == courseId);

                    if (course == null)
                        return _respone.SetFail("Khóa học không tồn tại");

                    chapter.CourseId = courseId.Value;
                }

                if (order.HasValue)
                {
                    var newOrder = order.Value;
                    var existOrder = await _context.Chapters
                        .AnyAsync(x =>
                            x.CourseId == (courseId ?? chapter.CourseId) &&
                            x.Order == newOrder &&
                            x.Id != chapter.Id);

                    if (existOrder)
                        return _respone.SetFail($"Vị trí số {newOrder} đã tồn tại trong khóa học!");

                    chapter.Order = newOrder;
                }

                if (title != null)
                    chapter.Title = title.Trim();

                chapter.UpdatedAt = DateTime.UtcNow;

                _context.Chapters.Update(chapter);
                await _context.SaveChangesAsync();

                return _respone.SetSuccess("Cập nhật chapter thành công", new
                {
                    chapter.Id,
                    chapter.Title,
                    chapter.Order,
                    chapter.CourseId,
                    chapter.CreatedAt,
                    chapter.UpdatedAt
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return _respone.SetFail("Có lỗi xảy ra khi cập nhật chương");
            }
        }
        public async Task<ResponseMessageResult> DeleteAsync(Guid id)
        {
            var chapter = await  _context.Chapters
                .FirstOrDefaultAsync(x => x.Id == id);

            if (chapter == null)
                return _respone.SetFail("Chương không tồn tại");

            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();
            return _respone.SetSuccess($"Xóa Chương {chapter.Order} thành công thành công");
        }
        public async Task<ResponseMessageResult> GetByCourseIdAsync(Guid courseId)
        {
            try
            {
                if (courseId == Guid.Empty)
                    return _respone.SetFail("Khóa học không hợp lệ");

                var course = await _context.Courses
                    .AnyAsync(x => x.Id == courseId);

                if (!course)
                    return _respone.SetFail("Khóa học không tồn tại");

                var result = await _context.Chapters
                    .Where(x => x.CourseId == courseId)
                    .OrderBy(x => x.Order)
                    .Select(x => new
                    {
                        x.Id,
                        x.Title,
                        x.Order,
                        x.CreatedAt,
                        x.UpdatedAt
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return _respone.SetSuccess("Lấy danh sách chương thành công", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return _respone.SetFail("Có lỗi xảy ra khi lấy danh sách chương theo khóa học");

            }
        }
    }
}
