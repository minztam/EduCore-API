using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EduCore.API.Data;
using EduCore.API.DTOs.HomeHero;
using EduCore.API.Entities;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories.Implementations
{
    public class HomeHeroRepository : IHomeHeroRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _respon;
        private readonly Cloudinary _cloudinary;

        public HomeHeroRepository(EduCoreDbContext context, ResponseMessageResult respon, Cloudinary cloudinary)
        {
            _context = context;
            _respon = respon;
            _cloudinary = cloudinary;
        }

        public async Task<ResponseMessageResult> GetAsync()
        {
            var hero = await _context.HomeHeroes
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return _respon.SetSuccess("OK", hero);
        }

        public async Task<ResponseMessageResult> SaveAsync(HomeHeroRequest req)
        {
            var hero = await _context.HomeHeroes.FirstOrDefaultAsync();
            if (hero == null)
            {
                hero = new HomeHero
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                };

                await _context.HomeHeroes.AddAsync(hero);
            }

            if (req.BannerImage != null && req.BannerImage.Length > 0)
            {
                // XÓA ảnh cũ (nếu có)
                if (!string.IsNullOrEmpty(hero.BannerImage))
                {
                    var publicId = GetPublicIdFromUrl(hero.BannerImage);

                    if (!string.IsNullOrEmpty(publicId))
                    {
                        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                    }
                }

                using var stream = req.BannerImage.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(req.BannerImage.FileName, stream),
                    Folder = "homehero"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                hero.BannerImage = uploadResult.SecureUrl.ToString();
            }

            hero.Title = req.Title;
            hero.SubTitle = req.SubTitle;
            hero.Description = req.Description;
            hero.PrimaryButtonText = req.PrimaryButtonText;
            hero.PrimaryButtonLink = req.PrimaryButtonLink;
            hero.SecondaryButtonText = req.SecondaryButtonText;
            hero.SecondaryButtonLink = req.SecondaryButtonLink;
            hero.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _respon.SetSuccess("Lưu Hero thành công", hero);
        }

        private string GetPublicIdFromUrl(string url)
        {
            try
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    return string.Empty;

                var uri = new Uri(url);
                var fileName = Path.GetFileNameWithoutExtension(uri.AbsolutePath);

                return $"homehero/{fileName}";
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<ResponseMessageResult> ToggleAsync()
        {
            var hero = await _context.HomeHeroes.FirstOrDefaultAsync();

            if (hero == null)
                return _respon.SetFail("Không tìm thấy Hero");

            hero.IsActive = !hero.IsActive;
            hero.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _respon.SetSuccess("Cập nhật trạng thái thành công", hero);
        }
    }
}
