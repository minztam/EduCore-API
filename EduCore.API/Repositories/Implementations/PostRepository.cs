using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EduCore.API.Data;
using EduCore.API.DTOs.Post;
using EduCore.API.Entities;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _response;
        private readonly Cloudinary _cloudinary;
        public PostRepository(EduCoreDbContext context, ResponseMessageResult response, Cloudinary cloudinary)
        {
            _context = context;
            _response = response;
            _cloudinary = cloudinary;
        }

        public async Task<ResponseMessageResult> ChangePublishStatusAsync(Guid id)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
                return _response.SetFail("Không tìm thấy bài viết", 404);

            post.IsPublished = !post.IsPublished;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _response.SetSuccess(post.IsPublished
                    ? "Xuất bản bài viết thành công"
                    : "Ẩn bài viết thành công");
        }

        public async Task<ResponseMessageResult> CreateAsync(PostRequest model)
        {
            try
            {
                var slugExists = await _context.Posts
                    .AnyAsync(x => x.Slug == model.Slug);

                if (slugExists)
                    return _response.SetFail("Slug bài viết đã tồn tại");

                if (model.CategoryId.HasValue)
                {
                    var category = await _context.Categories
                        .FirstOrDefaultAsync(x =>
                            x.Id == model.CategoryId &&
                            x.Type == "Blog");

                    if (category == null)
                        return _response.SetFail("Danh mục bài viết không tồn tại");
                }

                string? thumbnailUrl = null;

                if (model.Thumbnail != null && model.Thumbnail.Length > 0)
                {
                    using var thumbnailStream = model.Thumbnail.OpenReadStream();

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(model.Thumbnail.FileName, thumbnailStream),
                        Folder = "posts/thumbnails"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    thumbnailUrl = uploadResult.SecureUrl.AbsoluteUri;
                }

                var post = new Post
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Slug = model.Slug,
                    Summary = model.Summary,
                    Thumbnail = thumbnailUrl,
                    Content = model.Content,
                    AuthorName = model.AuthorName,
                    CategoryId = model.CategoryId,
                    IsPublished = model.IsPublished,
                    ViewCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Posts.AddAsync(post);
                await _context.SaveChangesAsync();

                var response = new
                {
                    post.Id,
                    post.Title,
                    post.Slug,
                    post.Thumbnail,
                    post.IsPublished,
                    post.CreatedAt
                };

                return _response.SetSuccess("Tạo bài viết thành công", response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return _response.SetFail("Lỗi khi tạo bài viết");
            }
        }

        public async Task<ResponseMessageResult> DeleteAsync(Guid id)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
                return _response.SetFail("Không tìm thấy bài viết", 404);

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return _response.SetSuccess("Xóa bài viết thành công");
        }

        public async Task<ResponseMessageResult> GetAllAsync(PostQuery query)
        {
            var q = _context.Posts
                .Include(x => x.Category)
                .Where(x => x.Category != null && x.Category.Type == "Blog")
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                q = q.Where(x =>
                    x.Title.Contains(query.Keyword) ||
                    x.Slug.Contains(query.Keyword) ||
                    (x.AuthorName != null && x.AuthorName.Contains(query.Keyword)));
            }

            if (query.CategoryId.HasValue)
            {
                q = q.Where(x => x.CategoryId == query.CategoryId);
            }

            if (query.IsPublished.HasValue)
            {
                q = q.Where(x => x.IsPublished == query.IsPublished);
            }

            var totalItems = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => x.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Slug,
                    x.Thumbnail,
                    x.IsPublished,
                    x.ViewCount,
                    x.CreatedAt,
                    x.IsFeatured,
                    Category = x.Category != null ? new
                    {
                        x.Category.Id,
                        x.Category.Name
                    } : null
                })
                .ToListAsync();

            var result = new
            {
                items,
                totalItems,
                page = query.Page,
                pageSize = query.PageSize
            };

            return _response.SetSuccess("Lấy danh sách bài viết thành công", result);
        }

        public async Task<ResponseMessageResult> GetByIdAsync(Guid id)
        {
            var posts = await _context.Posts
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (posts == null)
                return _response.SetFail("Không tìm thấy bài viết", 404);

            return _response.SetSuccess("Lấy bài viết thành công", posts);
        }

        public async Task<ResponseMessageResult> GetBySlugAsync(string slug)
        {
            var post = await _context.Posts
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Slug == slug && x.IsPublished);

            if (post == null)
                return _response.SetFail("Không tìm thấy bài viết", 404);

            post.ViewCount += 1;
            await _context.SaveChangesAsync();
            return _response.SetSuccess("Lấy chi tiết bài viết thành công", post);
        }

        public async Task<ResponseMessageResult> IncreaseViewAsync(string slug)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(x => x.Slug == slug && x.IsPublished);

            if (post == null)
                return _response.SetFail("Không tìm thấy bài viết", 404);

            post.ViewCount++;
            await _context.SaveChangesAsync();
            return _response.SetSuccess("Tăng lượt xem thành công", post.ViewCount);
        }

        public async Task<ResponseMessageResult> UpdateAsync(Guid id, PostRequest model)
        {
            try
            {
                var post = await _context.Posts
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (post == null)
                    return _response.SetFail("Bài viết không tồn tại");

                var slugExists = await _context.Posts
                    .AnyAsync(x => x.Id != id && x.Slug == model.Slug);

                if (slugExists)
                    return _response.SetFail("Slug bài viết đã tồn tại");

                if (model.CategoryId.HasValue)
                {
                    var category = await _context.Categories
                        .FirstOrDefaultAsync(x =>
                            x.Id == model.CategoryId &&
                            x.Type == "Blog");

                    if (category == null)
                        return _response.SetFail("Danh mục bài viết không tồn tại");
                }

                if (model.Thumbnail != null && model.Thumbnail.Length > 0)
                {
                    using var thumbnailStream = model.Thumbnail.OpenReadStream();

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(model.Thumbnail.FileName, thumbnailStream),
                        Folder = "posts/thumbnails"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    post.Thumbnail = uploadResult.SecureUrl.AbsoluteUri;
                }

                post.Title = model.Title;
                post.Slug = model.Slug;
                post.Summary = model.Summary;
                post.Content = model.Content;
                post.AuthorName = model.AuthorName;
                post.CategoryId = model.CategoryId;
                post.IsPublished = model.IsPublished;
                post.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return _response.SetSuccess("Cập nhật bài viết thành công", post);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return _response.SetFail("Lỗi khi cập nhật bài viết");
            }
        }

        public async Task<ResponseMessageResult> ChangeFeaturedStatusAsync(Guid id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
                return _response.SetFail("Không tìm thấy bài viết", 404);

            post.IsFeatured = !post.IsFeatured;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _response.SetSuccess(post.IsFeatured
                ? "Đã thiết lập bài viết nổi bật"
                : "Đã hủy bỏ trạng thái nổi bật");
        }
        
        public async Task<ResponseMessageResult> GetFeaturedPostsAsync(int count)
        {
            var items = await _context.Posts
                .Include(x => x.Category)
                .Where(x => x.IsPublished && x.IsFeatured)
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Slug,
                    x.Summary,
                    x.Thumbnail,
                    x.CreatedAt,
                    x.ViewCount,
                    CategoryName = x.Category != null ? x.Category.Name : ""
                })
                .ToListAsync();

            return _response.SetSuccess("Lấy danh sách bài viết nổi bật thành công", items);
        }
    }
}
