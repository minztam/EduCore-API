using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EduCore.API.Data;
using EduCore.API.Entities;
using EduCore.API.Repositories;
using EduCore.API.Repositories.ResponseMessage;
using EduCore.API.Service.Email;
using EduCore.API.Service.JWT;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using System.Security.Claims;

namespace EduCore.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _respon;
        private readonly EmailService _eService;
        private readonly JwtService _jwt;
        private readonly IConfiguration _configuration;
        private readonly Cloudinary _cloudinary;
        public UserRepository(EduCoreDbContext context, ResponseMessageResult respon, EmailService eService, JwtService jwt, IConfiguration configuration, Cloudinary cloudinary)
        {
            _context = context;
            _respon = respon;
            _eService = eService;
            _jwt = jwt;
            _configuration = configuration;
            _cloudinary = cloudinary;
        }

        public async Task<ResponseMessageResult> GetPagedUsersAsync(int pageIndex, int pageSize, string? keyword, string? role, bool? isActive)
        {
            try
            {
                var query = _context.Users.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                    query = query.Where(u => u.Name.Contains(keyword) || u.Email.Contains(keyword));

                if (!string.IsNullOrWhiteSpace(role))
                    query = query.Where(u => u.Role == role);

                if (isActive.HasValue)
                    query = query.Where(u => u.IsActive == isActive.Value);

                int totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return _respon.SetSuccess("Lấy dữ liệu phân trang thành công", new
                {
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    Items = items
                });
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi phân trang: {ex.Message}", 500);
            }
        }
        
        public async Task<ResponseMessageResult> RegisterAsync(string email, string password, string name)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return _respon.SetFail("Dữ liệu không hợp lệ");

            var isCheck = await _context.Users
                .AnyAsync(x => x.Email == email);

            if (isCheck)
                return _respon.SetFail("Email đã tồn tại");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = name,
                Password = password,

                Provider = "Local",
                Role = "User",
                IsActive = true,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Users.Add(user);  
            await _context.SaveChangesAsync();

            await _eService.SendWelcomeEmailAsync(user.Email, user.Name);
            return _respon.SetSuccess("Đăng ký thành công", user);
        }
        
        public async Task<ResponseMessageResult> LoginAsync(string email, string password) 
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email && x.Password == password);

            if (user == null)
                return _respon.SetFail("Sai email hoặc mật khẩu");

            if (!user.IsActive)
                return _respon.SetFail("Tài khoản bị khóa");

            var token = _jwt.GenerateJwtToken(user);

            return _respon.SetSuccess("Đăng nhập thành công",new
            {
                token,
                user.Id,
                user.Email,
                user.Name,
                user.AvatarUrl,
                user.Role
            });
        }
        
        public async Task<ResponseMessageResult> SearchByNameAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _respon.SetFail("Vui lòng nhập từ khóa");

            var users = await _context.Users
                .Where(x => EF.Functions.Like(x.Name, $"%{keyword}%"))
                .ToListAsync();

            Console.WriteLine(users);

            if (!users.Any())
                return _respon.SetSuccess("Không tìm thấy người dùng", users);

            return _respon.SetSuccess("Tìm thấy thành công", users);
        }
        
        public async Task<ResponseMessageResult> GetUserStatisticsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();

            var totalActive = await _context.Users
                .CountAsync(x => x.IsActive);

            var totalInactive = await _context.Users
                .CountAsync(x => !x.IsActive);

            var totalAdmin = await _context.Users
                .CountAsync(x => x.Role == "Admin");

            var totalUser = await _context.Users
                .CountAsync(x => x.Role == "User");

            return _respon.SetSuccess("Thống kê thành công", new
            {
                totalUsers,
                totalActive,
                totalInactive,
                totalAdmin,
                totalUser
            });
        }
        
        public async Task<ResponseMessageResult> ToggleUserStatusAsync(Guid id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return _respon.SetFail("Tài khoản không tồn tại");

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _respon.SetSuccess(user.IsActive ? "Mở khóa tài khoản thành công" 
                                                    : "Khóa tài khoản thành công", user);
        }
        
        public async Task<ResponseMessageResult> GoogleLoginAsync(string token)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[]
                    {
                        _configuration["Google:ClientId"]
                    }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email == payload.Email);

                bool isNewUser = false;

                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = payload.Email,
                        Name = payload.Name,
                        AvatarUrl = null,
                        Provider = "Google",
                        ProviderId = payload.Subject,
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _context.Users.AddAsync(user);
                    isNewUser = true;
                }
                else
                {
                    user.AvatarUrl = payload.Picture;
                    user.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                var jwtToken = _jwt.GenerateJwtToken(user);

                return _respon.SetSuccess("Đăng nhập thành công", new
                {
                    token = jwtToken,
                    isNewUser,
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.AvatarUrl
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google auth error: {ex.Message}");
                return _respon.SetFail("Google auth failed");
            }
        }
        
        public async Task<ResponseMessageResult> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (user == null)
                    return _respon.SetFail("Người dùng không tồn tại");

                return _respon.SetSuccess("Lấy thông tin người dùng thành công", user);
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Error: {ex.Message}", 500);
            }
        }
        
        public async Task<ResponseMessageResult> UpdateUserProfileAsync(Guid id,string? name = null,IFormFile? avata = null,string? password = null)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (user == null)
                    return _respon.SetFail("Người dùng không tồn tại");

                if (!string.IsNullOrWhiteSpace(name))
                    user.Name = name;

                if (!string.IsNullOrWhiteSpace(password))
                    user.Password = password;

                if (avata != null && avata.Length > 0)
                {
                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        var publicId = GetUserAvatarPublicId(user.AvatarUrl);
                        if (!string.IsNullOrEmpty(publicId))
                        {
                            await _cloudinary.DestroyAsync(
                                new DeletionParams(publicId));
                        }
                    }

                    using var avatarStream = avata.OpenReadStream();

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(avata.FileName, avatarStream),
                        Folder = "users/avatar"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    user.AvatarUrl = uploadResult.SecureUrl.AbsoluteUri;
                }

                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return _respon.SetSuccess("Cập nhật hồ sơ thành công", user);
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi cập nhật hồ sơ: {ex.Message}");
            }
        }

        public async Task<ResponseMessageResult> ChangeRoleAsync(Guid id, string newRole)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return _respon.SetFail("Người dùng không tồn tại");

                var validRoles = new List<string> { "Admin", "User", "Lecturer" }; 
                if (!validRoles.Contains(newRole))
                    return _respon.SetFail("Vai trò không hợp lệ");

                user.Role = newRole;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return _respon.SetSuccess($"Đã thay đổi vai trò của {user.Name} thành {newRole}", user);
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi đổi vai trò: {ex.Message}");
            }
        }

        public async Task<ResponseMessageResult> DeleteUserAsync(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return _respon.SetFail("Người dùng không tồn tại");

                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    var publicId = GetUserAvatarPublicId(user.AvatarUrl);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                    }
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return _respon.SetSuccess($"Đã xóa tài khoản {user.Email} khỏi hệ thống");
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi khi xóa người dùng: {ex.Message}");
            }
        }

        public async Task<ResponseMessageResult> BulkToggleStatusAsync(List<Guid> ids)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => ids.Contains(u.Id))
                    .ToListAsync();

                if (!users.Any())
                    return _respon.SetFail("Không tìm thấy người dùng nào trong danh sách");

                foreach (var user in users)
                {
                    user.IsActive = !user.IsActive; 
                    user.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return _respon.SetSuccess($"Đã cập nhật trạng thái cho {users.Count} người dùng");
            }
            catch (Exception ex)
            {
                return _respon.SetFail($"Lỗi thao tác hàng loạt: {ex.Message}");
            }
        }

        private string? GetUserAvatarPublicId(string url)
        {
            try
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    return null;

                var uri = new Uri(url);

                var segments = uri.Segments
                    .Select(s => s.Trim('/'))
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

                var idx = Array.FindIndex(segments, s => s == "users");
                if (idx == -1 || idx + 1 >= segments.Length)
                    return null;

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
