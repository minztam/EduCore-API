using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EduCore.API.Data;
using EduCore.API.DTOs.Course;
using EduCore.API.Entities;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories.Implementations
{
    public class CourseRepository : ICourseRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _respon;
        private readonly Cloudinary _cloudinary;
        public CourseRepository(EduCoreDbContext context, ResponseMessageResult respon, Cloudinary cloudinary)
        {
            _context = context;
            _respon = respon;
            _cloudinary = cloudinary;
        }

        public async Task<ResponseMessageResult> GetAllAsync()
        {
            try
            {
                var result = await _context.Courses
                    .Include(x => x.Instructor)
                    .Select(x => new
                    {
                        x.Id,
                        x.Title,
                        x.Slug,
                        x.Description,
                        x.Content,
                        x.Price,
                        x.Level,
                        x.Language,
                        x.TotalLessons,
                        x.TotalDuration,
                        x.ThumbnailUrl,
                        x.PreviewVideoUrl,
                        InstructorName = x.Instructor != null ? x.Instructor.Name : "",
                        x.IsPublished,
                        x.IsHost,
                        x.CreatedAt,
                        x.UpdatedAt,
                        x.CategoryId
                    })
                    .ToListAsync();

                return _respon.SetSuccess("Lấy danh sách khóa học thành công", result);
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi khi lấy khóa học: {ex.Message}");
            }
        }
        public async Task<ResponseMessageResult> CreateAsync(string instructorId, string title, string slug, string? desc, string content, IFormFile? thumb, IFormFile? preview, decimal price, string leve, string lang, Guid? categoryId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            string? thumbPublicId = null;
            string? previewPublicId = null;

            try
            {
                var instructor = await _context.Users.FindAsync(Guid.Parse(instructorId));
                if (instructor == null || instructor.Role != "Admin")
                    return _respon.SetFail("Người tạo không tồn tại hoặc không phải Instructor");

                Categories? category = null;
                if (categoryId.HasValue)
                {
                    category = await _context.Categories.FindAsync(categoryId.Value);
                    if (category == null)
                        return _respon.SetFail("Danh mục không tồn tại");
                }

                string? thumbUrl = null;
                string? previewUrl = null;

                if (thumb != null && thumb.Length > 0)
                {
                    using var thumbStream = thumb.OpenReadStream();

                    var uploadResult = await _cloudinary.UploadAsync(new ImageUploadParams
                    {
                        File = new FileDescription(thumb.FileName, thumbStream),
                        Folder = "courses/thumbnails"
                    });

                    thumbUrl = uploadResult.SecureUrl.AbsoluteUri;
                    thumbPublicId = uploadResult.PublicId;
                }

                if (preview != null && preview.Length > 0)
                {
                    using var previewStream = preview.OpenReadStream();

                    var uploadResult = await _cloudinary.UploadAsync(new VideoUploadParams
                    {
                        File = new FileDescription(preview.FileName, previewStream),
                        Folder = "courses/previews"
                    });

                    previewUrl = uploadResult.SecureUrl.AbsoluteUri;
                    previewPublicId = uploadResult.PublicId;
                }

                var course = new Course
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Slug = slug,
                    Description = desc ?? string.Empty,
                    Content = content,
                    ThumbnailUrl = thumbUrl,
                    PreviewVideoUrl = previewUrl,
                    Price = price,
                    Level = leve,
                    Language = lang,
                    CreatedBy = Guid.Parse(instructorId),
                    Instructor = instructor,
                    CategoryId = categoryId,
                    Category = category,
                    CreatedAt = DateTime.UtcNow,
                    IsPublished = false
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return _respon.SetSuccess("Tạo khóa học thành công", course);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (!string.IsNullOrEmpty(thumbPublicId))
                {
                    await _cloudinary.DestroyAsync(new DeletionParams(thumbPublicId)
                    {
                        ResourceType = ResourceType.Image
                    });
                }

                if (!string.IsNullOrEmpty(previewPublicId))
                {
                    await _cloudinary.DestroyAsync(new DeletionParams(previewPublicId)
                    {
                        ResourceType = ResourceType.Video
                    });
                }

                return _respon.SetFail($"Lỗi khi tạo khóa học: {ex.Message}");
            }
        }
        public async Task<ResponseMessageResult> UpdateAsync(Guid id, string? instructorId, string? title, string? slug, string? desc, string? content, IFormFile? thumb, IFormFile? preview, decimal? price, string? level, string? lang, Guid? categoryId)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (course == null)
                    return _respon.SetFail("Khóa học không tồn tại");

                if (string.IsNullOrEmpty(instructorId))
                    return _respon.SetFail("InstructorId không hợp lệ");

                if (course.CreatedBy != Guid.Parse(instructorId))
                    return _respon.SetFail("Bạn không có quyền cập nhật khóa học này");

                if (!string.IsNullOrWhiteSpace(title))
                    course.Title = title;

                if (!string.IsNullOrWhiteSpace(slug))
                    course.Slug = slug;

                if (!string.IsNullOrWhiteSpace(desc))
                    course.Description = desc;

                if (!string.IsNullOrWhiteSpace(content))
                    course.Content = content;

                if (price.HasValue)
                    course.Price = price.Value;

                if (!string.IsNullOrWhiteSpace(level))
                    course.Level = level;

                if (!string.IsNullOrWhiteSpace(lang))
                    course.Language = lang;

                if (categoryId.HasValue)
                {
                    var categoryExists = await _context.Categories
                        .AnyAsync(x => x.Id == categoryId.Value);

                    if (!categoryExists)
                        return _respon.SetFail("Danh mục không tồn tại");

                    course.CategoryId = categoryId.Value;
                }

                course.UpdatedAt = DateTime.UtcNow;

                if (thumb != null && thumb.Length > 0)
                {
                    if (!string.IsNullOrEmpty(course.ThumbnailUrl))
                    {
                        var publicId = GetCloudinaryPublicId(course.ThumbnailUrl);
                        if (!string.IsNullOrEmpty(publicId))
                        {
                            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                        }
                    }

                    using var thumbStream = thumb.OpenReadStream();
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(thumb.FileName, thumbStream),
                        Folder = "courses/thumbnails"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    course.ThumbnailUrl = uploadResult.SecureUrl.AbsoluteUri;
                }

                if (preview != null && preview.Length > 0)
                {
                    if (!string.IsNullOrEmpty(course.PreviewVideoUrl))
                    {
                        var publicId = GetCloudinaryPublicId(course.PreviewVideoUrl);
                        if (!string.IsNullOrEmpty(publicId))
                        {
                            await _cloudinary.DestroyAsync(
                                new DeletionParams(publicId)
                                {
                                    ResourceType = ResourceType.Video
                                });
                        }
                    }

                    using var previewStream = preview.OpenReadStream();
                    var uploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(preview.FileName, previewStream),
                        Folder = "courses/previews"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    course.PreviewVideoUrl = uploadResult.SecureUrl.AbsoluteUri;
                }

                _context.Courses.Update(course);
                await _context.SaveChangesAsync();

                var resultDto = new UpdateCourseRespone
                {
                    Id = course.Id,
                    Title = course.Title,
                    Slug = course.Slug,
                    Description = course.Description,
                    Content = course.Content,
                    ThumbnailUrl = course.ThumbnailUrl,
                    PreviewVideoUrl = course.PreviewVideoUrl,
                    Price = course.Price,
                    Level = course.Level,
                    Language = course.Language,
                    TotalLessons = course.TotalLessons,
                    TotalDuration = course.TotalDuration,
                    IsPublished = course.IsPublished,
                    CreatedAt = course.CreatedAt,
                    UpdatedAt = course.UpdatedAt,
                    CategoryId = course.CategoryId,
                    CategoryName = course.Category?.Name!,
                    Instructor = new InstructorRespone
                    {
                        Id = course.Instructor!.Id,
                        Name = course.Instructor.Name,
                    }
                };

                return _respon.SetSuccess("Cập nhật khóa học thành công", resultDto);
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi khi cập nhật khóa học: {ex.Message}");
            }
        }
        public async Task<ResponseMessageResult> TogglePublishAsync(Guid id)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (course == null)
                    return _respon.SetFail("Khóa học không tồn tại");

                course.IsPublished = !course.IsPublished;
                course.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return _respon.SetSuccess(course.IsPublished ? 
                    "Đã công khai khóa học" : "Đã ẩn khóa học", 
                    new 
                    {
                        course.Id,
                        course.IsPublished
                    }
                );
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi: {ex.Message}");
            }
        }
        public async Task<ResponseMessageResult> ToggleHotAsync(Guid id)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (course == null)
                    return _respon.SetFail("Khóa học không tồn tại");

                course.IsHost = !course.IsHost;
                course.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return _respon.SetSuccess(course.IsHost ?
                    "Đã bật nổi bật" : "Đã tắt nổi bật", new { course.Id, course.IsHost, course.UpdatedAt });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi toggle nổi bật: {ex}");
                return _respon.SetFail($"Lỗi khi thực hiện thao tác");
            }
        }
        public async Task<ResponseMessageResult> SearchAsync(string key, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Courses.AsQueryable();

                if (!string.IsNullOrEmpty(key))
                {
                    key = key.ToLower();

                    query = query.Where(x =>
                        EF.Functions.Like(x.Title, $"%{key}%")
                    );
                }

                var data = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        x.Title,
                        x.Slug,
                        x.Price,
                        x.Level,
                        x.Language,
                        x.IsPublished
                    })
                    .ToListAsync();

                return _respon.SetSuccess("Tìm kiếm thành công", new { page, pageSize, data});
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi: {ex.Message}");
            }
        }
        public async Task<ResponseMessageResult> DeleteAsync(Guid id)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (course == null)
                    return _respon.SetFail("Khóa học không tồn tại");

                if (!string.IsNullOrEmpty(course.ThumbnailUrl))
                {
                    var publicId = GetCloudinaryPublicId(course.ThumbnailUrl);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        await _cloudinary.DestroyAsync(
                            new DeletionParams(publicId)
                        );
                    }
                }

                if (!string.IsNullOrEmpty(course.ThumbnailUrl))
                {
                    var publicId = GetCloudinaryPublicId(course.ThumbnailUrl);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        await _cloudinary.DestroyAsync(
                            new DeletionParams(publicId)
                        );
                    }
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                return _respon.SetSuccess("Xóa khóa học thành công", course);
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi: {ex.Message}");
            }
        }
        public async Task<ResponseMessageResult> FilterAsync(string? level,string? price,bool? isPublished,int page,int pageSize)
        {
            try
            {
                var query = _context.Courses.AsQueryable();

                if (!string.IsNullOrEmpty(level))
                {
                    query = query.Where(x => x.Level == level);
                }

                if (isPublished.HasValue)
                {
                    query = query.Where(x => x.IsPublished == isPublished.Value);
                }

                if (!string.IsNullOrEmpty(price))
                {
                    switch (price)
                    {
                        case "free":
                            query = query.Where(x => x.Price == 0);
                            break;

                        case "paid":
                            query = query.Where(x => x.Price > 0);
                            break;

                        case "low":
                            query = query.Where(x => x.Price > 0 && x.Price < 500000);
                            break;

                        case "high":
                            query = query.Where(x => x.Price >= 500000);
                            break;
                    }
                }
                query = query.OrderByDescending(x => x.CreatedAt);

                var total = await query.CountAsync();

                var data = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return _respon.SetSuccess("Lấy dữ liệu thành công", new
                {
                    total,
                    page,
                    pageSize,
                    data
                });
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi: {ex.Message}");
            }
        }
        public async Task<ResponseMessageResult> GetByIdAsync(Guid id)
        {
            var course = await _context.Courses
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (course == null)
                return _respon.SetFail("Không tìm thấy khóa học", 404);

            return _respon.SetSuccess("Lấy chi tiết khóa học thành công", course);
        }
        public async Task<ResponseMessageResult> GetBySlugAsync(string slug)
        {
            var course = await _context.Courses
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Slug == slug);

            if (course == null)
                return _respon.SetFail("Không tìm thấy khóa học", 404);

            return _respon.SetSuccess("Lấy chi tiết khóa học thành công", course);
        }
        private string? GetCloudinaryPublicId(string url)
        {
            try
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    return null;

                var uri = new Uri(url);
                var segments = uri.Segments.Select(s => s.Trim('/')).Where(s => !string.IsNullOrEmpty(s)).ToArray();

                var idx = Array.FindIndex(segments, s => s == "courses");
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
