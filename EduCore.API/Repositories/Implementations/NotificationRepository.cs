using EduCore.API.Data;
using EduCore.API.DTOs.Notification;
using EduCore.API.Entities;
using EduCore.API.Hubs;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _response;
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationRepository(EduCoreDbContext context, ResponseMessageResult response, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _response = response;
            _hubContext = hubContext;
        }
        public async Task<ResponseMessageResult> GetHeaderNotificationsAsync()
        {
            var notifications = await _context.Notifications
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5) 
                .Select(x => new 
                {
                    Id = x.Id,
                    SenderName = x.User != null ? x.User.Name : "Hệ thống",
                    SenderAvatar = x.User != null ? x.User.AvatarUrl : null,
                    Content = x.Content,
                    Type = x.Type,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return _response.SetSuccess("Lấy 5 thông báo mới nhất cho Header", notifications);
        }
        public async Task<ResponseMessageResult> GetAllForPageAsync(int page = 1, int pageSize = 10)
        {
            var totalCount = await _context.Notifications.CountAsync();

            var notifications = await _context.Notifications
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NotificationResponse
                {
                    Id = x.Id,
                    SenderId = x.SenderId,
                    SenderName = x.User != null ? x.User.Name : "Hệ thống",
                    SenderAvatar = x.User != null ? x.User.AvatarUrl : null,
                    Content = x.Content,
                    Type = x.Type,
                    RedirectUrl = x.RedirectUrl,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var data = new
            {
                TotalItems = totalCount,
                Items = notifications
            };

            return _response
                .SetSuccess("Lấy danh sách thông báo thành công", data);
        }
        public async Task<ResponseMessageResult> AddNotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            var sender = await _context.Users
                .Where(u => u.Id == notification.SenderId)
                .Select(u => new { u.Name, u.AvatarUrl })
                .FirstOrDefaultAsync();

            var notificationDto = new NotificationResponse
            {
                Id = notification.Id,
                SenderId = notification.SenderId,
                SenderName = sender?.Name ?? "Hệ thống",
                SenderAvatar = sender?.AvatarUrl,
                Content = notification.Content,
                Type = notification.Type,
                RedirectUrl = notification.RedirectUrl,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notificationDto);
            return _response.SetSuccess("Thêm thông báo thành công", notification);
        }
        public async Task<ResponseMessageResult> MarkAsReadAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return _response.SetFail("Không tìm thấy thông báo", 404);

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return _response.SetSuccess("Đã đánh dấu thông báo là đã đọc", null);
        }
    }
}
