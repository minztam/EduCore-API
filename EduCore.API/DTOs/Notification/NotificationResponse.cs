namespace EduCore.API.DTOs.Notification
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAvatar { get; set; }

        public string? Content { get; set; }
        public string? Type { get; set; }
        public string? RedirectUrl { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
