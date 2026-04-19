using EduCore.API.DTOs.Chat;
using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface IChatRepository
    {
        // Quản lý phòng
        Task<ResponseMessageResult> GetUserRoomsAsync(Guid userId);
        Task<ResponseMessageResult> GetRoomDetailAsync(int roomId, Guid userId);
        Task<ResponseMessageResult> CreatePrivateRoomAsync(Guid user1Id, Guid user2Id);

        // Quản lý tin nhắn
        Task<ResponseMessageResult> SendMessageAsync(ChatMessageRequest model, Guid senderId);
        Task<ResponseMessageResult> GetMessagesAsync(int roomId, Guid userId, int page = 1, int pageSize = 30);
        Task<ResponseMessageResult> DeleteMessageAsync(int messageId, Guid userId);
    }
}
