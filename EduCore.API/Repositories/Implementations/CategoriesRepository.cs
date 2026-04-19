using EduCore.API.Data;
using EduCore.API.Entities;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories.Implementations
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _respon;

        public CategoriesRepository(
            EduCoreDbContext context,
            ResponseMessageResult respon)
        {
            _context = context;
            _respon = respon;
        }

        public async Task<ResponseMessageResult> GetAllAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Slug,
                    x.Type,
                    x.IsActive,
                    x.SortOrder,
                    x.ParentId,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync();

            return _respon.SetSuccess("Lấy dữ liệu thành công", categories);
        }
        public async Task<ResponseMessageResult> CreateAsync(string name, string slug,string type,Guid? parentId,int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(name))
                return _respon.SetFail("Tên danh mục không hợp lệ");

            if (string.IsNullOrWhiteSpace(slug))
                return _respon.SetFail("Slug không hợp lệ");

            var isSlugExist = await _context.Categories
                .AnyAsync(x => x.Slug == slug);

            if (isSlugExist)
                return _respon.SetFail("Slug đã tồn tại");

            if (parentId.HasValue)
            {
                var parentExists = await _context.Categories
                    .AnyAsync(x => x.Id == parentId.Value);

                if (!parentExists)
                    return _respon.SetFail("Danh mục cha không tồn tại");
            }

            var category = new Categories
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug,
                Type = type,
                ParentId = parentId,
                SortOrder = sortOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return _respon.SetSuccess("Tạo danh mục thành công", category);
        }
        public async Task<ResponseMessageResult> UpdateAsync(Guid id,string name,string slug, string type,Guid? parentId,int sortOrder)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return _respon.SetFail("Danh mục không tồn tại");

            var isSlugExist = await _context.Categories
                .AnyAsync(x => x.Slug == slug && x.Id != id);

            if (isSlugExist)
                return _respon.SetFail("Slug đã tồn tại");

            if (parentId == id)
                return _respon.SetFail("Không thể chọn chính nó làm danh mục cha");

            category.Name = name;
            category.Slug = slug;
            category.Type = type;
            category.ParentId = parentId;
            category.SortOrder = sortOrder;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _respon.SetSuccess("Cập nhật thành công", category);
        }
        public async Task<ResponseMessageResult> DeleteAsync(Guid id)
        {
            var category = await _context.Categories
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return _respon.SetFail("Danh mục không tồn tại");

            if (category.Children.Any())
                return _respon.SetFail("Không thể xóa vì có danh mục con");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return _respon.SetSuccess("Xóa thành công", category);
        }
        public async Task<ResponseMessageResult> ToggleStatusAsync(Guid id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return _respon.SetFail("Danh mục không tồn tại");

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _respon.SetSuccess(
                category.IsActive ? "Mở danh mục thành công" : "Khóa danh mục thành công",
                category
            );
        }
    }
}