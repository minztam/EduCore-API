using EduCore.API.Entities;
using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<ResponseMessageResult> GetAllForPageAsync(int page = 1, int pageSize = 10);
        Task<ResponseMessageResult> GetHeaderNotificationsAsync();
        Task<ResponseMessageResult> AddNotificationAsync(Notification notification);
        Task<ResponseMessageResult> MarkAsReadAsync(Guid id);
    }
}
