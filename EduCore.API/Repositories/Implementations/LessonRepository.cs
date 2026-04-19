using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EduCore.API.Data;
using EduCore.API.DTOs.Lesson;
using EduCore.API.Entities;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories.Implementations
{
    public class LessonRepository : ILessonRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _response;
        private readonly Cloudinary _cloudinary;
        public LessonRepository(EduCoreDbContext context, ResponseMessageResult response, Cloudinary cloudinary)
        {
            _context = context;
            _response = response;
            _cloudinary = cloudinary;
        }

        public async Task<ResponseMessageResult> GetByChapterIdAsync(Guid chapterId)
        {
            try
            {
                var lessons = await _context.Lessons
                    .Where(x => x.ChapterId == chapterId)
                    .OrderBy(x => x.OrderIndex)
                    .Select(x => new
                    {
                        x.Id,
                        x.Title,
                        x.Slug,
                        x.ContentType,
                        x.VideoUrl,
                        x.DocumentUrl,
                        x.Content,
                        x.OrderIndex,
                        x.IsPublished,
                        x.Duration,
                        x.IsFreePreview
                    })
                    .ToListAsync();

                if (!lessons.Any())
                    return _response.SetSuccess("Chưa có dữ liệu", lessons);

                return _response.SetSuccess("Lấy danh sách thành công", lessons);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return _response.SetFail("Lỗi khi lấy danh sách");
            }
        }
        public async Task<ResponseMessageResult> GetByIdAsync(Guid id)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(x => x.Id == id);

            if (lesson == null)
                return _response.SetFail("Bài học không tồn tại");

            return _response.SetSuccess("Lấy bài học thành công", lesson);
        }
        public async Task<ResponseMessageResult> CreateAsync(CreateLessonRequest model)
        {
            // Sử dụng transaction để đảm bảo nếu tăng số lượng bài học lỗi thì không tạo bài học và ngược lại
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra Chapter tồn tại và lấy luôn thông tin CourseId
                var chapter = await _context.Chapters
                    .FirstOrDefaultAsync(x => x.Id == model.ChapterId);

                if (chapter == null)
                    return _response.SetFail("Chapter không tồn tại");

                // 2. Kiểm tra Slug bài học đã tồn tại trong Chapter này chưa
                var slugExists = await _context.Lessons
                    .AnyAsync(x => x.ChapterId == model.ChapterId && x.Slug == model.Slug);

                if (slugExists)
                    return _response.SetFail("Slug bài học đã tồn tại trong chương này");

                // 3. Xử lý Upload Video/Tài liệu lên Cloudinary
                string? videoUrl = null;
                string? documentUrl = null;

                if (model.VideoUrl != null && model.VideoUrl.Length > 0)
                {
                    using var videoStream = model.VideoUrl.OpenReadStream();
                    var uploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(model.VideoUrl.FileName, videoStream),
                        Folder = "lessons/videos"
                    };
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    videoUrl = uploadResult.SecureUrl.AbsoluteUri;
                }

                if (model.DocumentUrl != null && model.DocumentUrl.Length > 0)
                {
                    using var documentStream = model.DocumentUrl.OpenReadStream();
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(model.DocumentUrl.FileName, documentStream),
                        Folder = "lessons/documents"
                    };
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    documentUrl = uploadResult.SecureUrl.AbsoluteUri;
                }

                // 4. Tạo đối tượng Lesson mới
                var lesson = new Lesson
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Slug = model.Slug,
                    ContentType = model.ContentType,
                    VideoUrl = videoUrl,
                    DocumentUrl = documentUrl,
                    Content = model.Content,
                    OrderIndex = model.OrderIndex,
                    ChapterId = model.ChapterId,
                    IsPublished = false
                };

                // 5. Lưu bài học vào Database
                await _context.Lessons.AddAsync(lesson);

                // 6. Cập nhật tăng số lượng bài học (TotalLessons) ở bảng Course
                var course = await _context.Courses
                    .FirstOrDefaultAsync(x => x.Id == chapter.CourseId);

                if (course != null)
                {
                    course.TotalLessons += 1;
                    course.UpdatedAt = DateTime.Now;
                    _context.Courses.Update(course);
                }

                // Lưu tất cả thay đổi
                await _context.SaveChangesAsync();

                // Xác nhận hoàn tất transaction
                await transaction.CommitAsync();

                // 7. Map dữ liệu trả về cho Client
                var response = new CreateLessonResponse
                {
                    Id = lesson.Id,
                    Title = lesson.Title,
                    Slug = lesson.Slug,
                    ContentType = lesson.ContentType,
                    VideoUrl = lesson.VideoUrl,
                    DocumentUrl = lesson.DocumentUrl,
                    OrderIndex = lesson.OrderIndex,
                    ChapterId = lesson.ChapterId
                };

                return _response.SetSuccess("Tạo bài học thành công", response);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, hoàn tác toàn bộ thay đổi trong database
                await transaction.RollbackAsync();
                Console.WriteLine($"Error at CreateLesson: {ex.Message}");
                return _response.SetFail("Lỗi hệ thống khi tạo bài học");
            }
        }
        public async Task<ResponseMessageResult> UpdateAsync(UpdateLessonRequest model)
        {
            try
            {
                var lesson = await _context.Lessons
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (lesson == null)
                    return _response.SetFail("Bài học không tồn tại");

                if (!string.IsNullOrWhiteSpace(model.Slug))
                {
                    var slugExists = await _context.Lessons
                        .AnyAsync(x => x.ChapterId == lesson.ChapterId
                                    && x.Slug == model.Slug
                                    && x.Id != model.Id);

                    if (slugExists)
                        return _response.SetFail("Slug bài học đã tồn tại");
                }

                string? videoUrl = lesson.VideoUrl;
                string? documentUrl = lesson.DocumentUrl;

                if (model.VideoFile != null && model.VideoFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(lesson.VideoUrl))
                    {
                        var publicId = GetByPublicId(lesson.VideoUrl);
                        if (!string.IsNullOrEmpty(publicId))
                        {
                            await _cloudinary.DestroyAsync(new DeletionParams(publicId)
                            {
                                ResourceType = ResourceType.Video
                            });
                        }
                    }

                    using var stream = model.VideoFile.OpenReadStream();
                    var uploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(model.VideoFile.FileName, stream),
                        Folder = "lessons/videos"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    videoUrl = uploadResult.SecureUrl.AbsoluteUri;
                }

                if (model.DocumentFile != null && model.DocumentFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(lesson.DocumentUrl))
                    {
                        var publicId = GetByPublicId(lesson.DocumentUrl);
                        if (!string.IsNullOrEmpty(publicId))
                        {
                            await _cloudinary.DestroyAsync(new DeletionParams(publicId)
                            {
                                ResourceType = ResourceType.Raw
                            });
                        }
                    }

                    using var stream = model.DocumentFile.OpenReadStream();
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(model.DocumentFile.FileName, stream),
                        Folder = "lessons/documents"
                    };
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    documentUrl = uploadResult.SecureUrl.AbsoluteUri;
                }

                if (!string.IsNullOrWhiteSpace(model.Title))
                    lesson.Title = model.Title;

                if (!string.IsNullOrWhiteSpace(model.Slug))
                    lesson.Slug = model.Slug;

                if (!string.IsNullOrWhiteSpace(model.ContentType))
                    lesson.ContentType = model.ContentType;

                if (model.Content != null)
                    lesson.Content = model.Content;

                if (model.OrderIndex.HasValue)
                    lesson.OrderIndex = model.OrderIndex.Value;

                lesson.IsPublished = model.IsPublished;
                if (model.Duration.HasValue)
                    lesson.Duration = model.Duration.Value;

                lesson.VideoUrl = videoUrl;
                lesson.DocumentUrl = documentUrl;

                await _context.SaveChangesAsync();

                var response = new UpdateLessonResponse
                {
                    Id = lesson.Id,
                    Title = lesson.Title,
                    Slug = lesson.Slug,
                    ContentType = lesson.ContentType,
                    VideoUrl = lesson.VideoUrl,
                    DocumentUrl = lesson.DocumentUrl,
                    OrderIndex = lesson.OrderIndex,
                    ChapterId = lesson.ChapterId,
                    IsPublished = lesson.IsPublished,
                    Duration = lesson.Duration
                };

                return _response.SetSuccess("Cập nhật bài học thành công", response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return _response.SetFail("Lỗi khi cập nhật bài học");
            }
        }
        public async Task<ResponseMessageResult> DeleteAsync(Guid id)
        {
            try
            {
                var lesson = await _context.Lessons
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (lesson == null)
                    return _response.SetFail("Bài học không tồn tại");

                if (!string.IsNullOrEmpty(lesson.VideoUrl))
                {
                    var publicId = GetByPublicId(lesson.VideoUrl);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        await _cloudinary.DestroyAsync(new DeletionParams(publicId)
                        {
                            ResourceType = ResourceType.Video
                        });
                    }
                }

                if (!string.IsNullOrEmpty(lesson.DocumentUrl))
                {
                    var publicId = GetByPublicId(lesson.DocumentUrl);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        await _cloudinary.DestroyAsync(new DeletionParams(publicId)
                        {
                            ResourceType = ResourceType.Raw
                        });
                    }
                }

                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
                return _response.SetSuccess("Xóa bài học thành công");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return _response.SetFail("Lỗi khi xóa bài học");
            }
        }

        private string? GetByPublicId(string url)
        {
            try
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    return null;

                var uri = new Uri(url);
                var segments = uri.Segments.Select(s => s.Trim('/')).Where(s => !string.IsNullOrEmpty(s)).ToArray();

                var idx = Array.FindIndex(segments, s => s == "lessons");
                if (idx == -1 || idx + 2 >= segments.Length) return null;

                var folder = segments[idx] + "/" + segments[idx + 1];
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(segments[^1]);

                return $"{folder}/{fileNameWithoutExt}";
            }
            catch
            {
                return null;
            }
        }
    }
}
