using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EduCore.API.Data;
using EduCore.API.DTOs.Chat;
using EduCore.API.Entities;
using EduCore.API.Hubs;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories.Implementations
{
    public class ChatRepository : IChatRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _response;
        private readonly Cloudinary _cloudinary;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatRepository(EduCoreDbContext context, ResponseMessageResult response, Cloudinary cloudinary, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _response = response;
            _cloudinary = cloudinary;
            _hubContext = hubContext;
        }

        // 1. Gửi tin nhắn (Hỗ trợ cả Text và Image)
        public async Task<ResponseMessageResult> SendMessageAsync(ChatMessageRequest model, Guid senderId)
        {
            try
            {
                // Kiểm tra xem User có trong phòng không
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(x => x.ChatRoomId == model.ChatRoomId && x.UserId == senderId);

                if (!isParticipant)
                    return _response.SetFail("Bạn không có quyền gửi tin nhắn vào phòng này", 403);

                string content = model.Content ?? "";
                string messageType = "Text";

                // Xử lý nếu có hình ảnh đính kèm
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    using var stream = model.ImageFile.OpenReadStream();
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(model.ImageFile.FileName, stream),
                        Folder = "chat/images"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    content = uploadResult.SecureUrl.AbsoluteUri;
                    messageType = "Image";
                }

                var message = new ChatMessage
                {
                    ChatRoomId = model.ChatRoomId,
                    SenderId = senderId,
                    Content = content,
                    MessageType = messageType,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.ChatMessages.AddAsync(message);
                await _context.SaveChangesAsync();

                // 3. Gửi tin nhắn Real-time đến những người đang trong phòng
                // Lấy thêm thông tin Sender để hiển thị ngay trên UI của người nhận
                var sender = await _context.Users
                    .Where(u => u.Id == senderId)
                    .Select(u => new { u.Id, u.Name, u.AvatarUrl })
                    .FirstOrDefaultAsync();

                var messageResponse = new
                {
                    message.Id,
                    message.Content,
                    message.MessageType,
                    message.CreatedAt,
                    message.ChatRoomId,
                    Sender = sender
                };

                // Bắn tín hiệu đến Group có tên là RoomId
                await _hubContext.Clients.Group(model.ChatRoomId.ToString())
                    .SendAsync("ReceiveMessage", messageResponse);

                return _response.SetSuccess("Gửi tin nhắn thành công", messageResponse);
            }
            catch (Exception ex)
            {
                return _response.SetFail("Lỗi hệ thống: " + ex.Message);
            }
        }

        // 2. Lấy danh sách phòng chat của User
        public async Task<ResponseMessageResult> GetUserRoomsAsync(Guid userId)
        {
            var rooms = await _context.ChatParticipants
                .Where(p => p.UserId == userId)
                .Include(p => p.ChatRoom)
                .Select(p => new
                {
                    RoomId = p.ChatRoomId,
                    p.ChatRoom!.Name,
                    p.ChatRoom!.IsGroup,
                    p.ChatRoom!.CourseId,
                    // Lấy tin nhắn mới nhất
                    LastMessage = _context.ChatMessages
                        .Where(m => m.ChatRoomId == p.ChatRoomId)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => new { m.Content, m.CreatedAt, m.MessageType })
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.LastMessage != null ? x.LastMessage.CreatedAt : DateTime.MinValue)
                .ToListAsync();

            return _response.SetSuccess("Lấy danh sách phòng chat thành công", rooms);
        }

        // 3. Lấy lịch sử tin nhắn của một phòng
        public async Task<ResponseMessageResult> GetMessagesAsync(int roomId, Guid userId, int page = 1, int pageSize = 30)
        {
            // Kiểm tra quyền truy cập phòng
            var isParticipant = await _context.ChatParticipants
                .AnyAsync(x => x.ChatRoomId == roomId && x.UserId == userId);

            if (!isParticipant)
                return _response.SetFail("Bạn không có quyền xem tin nhắn trong phòng này", 403);
            var query = _context.ChatMessages
                .Where(m => m.ChatRoomId == roomId && !m.IsDeleted)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.CreatedAt);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.Content,
                    m.MessageType,
                    m.CreatedAt,
                    Sender = new
                    {
                        m.Sender!.Id,
                        m.Sender!.Name,
                        m.Sender!.AvatarUrl
                    }
                })
                .ToListAsync();

            return _response.SetSuccess("Lấy tin nhắn thành công", items.OrderBy(x => x.CreatedAt));
        }

        // 4. Tạo phòng chat cá nhân (nếu chưa có)
        public async Task<ResponseMessageResult> CreatePrivateRoomAsync(Guid user1Id, Guid user2Id)
        {
            // Kiểm tra xem đã có phòng 1:1 giữa 2 người này chưa
            var existingRoom = await _context.ChatParticipants
                .Where(p => p.UserId == user1Id || p.UserId == user2Id)
                .GroupBy(p => p.ChatRoomId)
                .Where(g => g.Count() == 2)
                .Select(g => g.Key)
                .FirstOrDefaultAsync(roomId => _context.ChatRooms.Any(r => r.Id == roomId && !r.IsGroup));

            if (existingRoom != 0)
                return _response.SetSuccess("Phòng đã tồn tại", new { RoomId = existingRoom });

            // Nếu chưa có thì tạo mới
            var room = new ChatRoom
            {
                IsGroup = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ChatRooms.AddAsync(room);
            await _context.SaveChangesAsync();

            await _context.ChatParticipants.AddRangeAsync(
                new ChatParticipant { ChatRoomId = room.Id, UserId = user1Id },
                new ChatParticipant { ChatRoomId = room.Id, UserId = user2Id }
            );
            await _context.SaveChangesAsync();

            return _response.SetSuccess("Tạo phòng chat 1:1 thành công", new { RoomId = room.Id });
        }

        public async Task<ResponseMessageResult> GetRoomDetailAsync(int roomId, Guid userId)
        {
            try
            {
                var room = await _context.ChatRooms
                    .FirstOrDefaultAsync(r => r.Id == roomId);

                if (room == null)
                    return _response.SetFail("Không tìm thấy phòng chat", 404);

                // Kiểm tra xem User có thuộc phòng này không
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(p => p.ChatRoomId == roomId && p.UserId == userId);

                if (!isParticipant)
                    return _response.SetFail("Bạn không có quyền truy cập phòng này", 403);

                object result;

                if (room.IsGroup)
                {
                    // Nếu là nhóm (Khóa học)
                    result = new
                    {
                        room.Id,
                        room.Name,
                        room.IsGroup,
                        room.CourseId,
                        MemberCount = await _context.ChatParticipants.CountAsync(p => p.ChatRoomId == roomId)
                    };
                }
                else
                {
                    // Nếu là chat 1:1, lấy thông tin của người kia
                    var otherUser = await _context.ChatParticipants
                        .Where(p => p.ChatRoomId == roomId && p.UserId != userId)
                        .Include(p => p.User)
                        .Select(p => new
                        {
                            p.User!.Id,
                            p.User.Name,
                            p.User.AvatarUrl
                        })
                        .FirstOrDefaultAsync();

                    result = new
                    {
                        room.Id,
                        room.IsGroup,
                        OtherUser = otherUser
                    };
                }

                return _response.SetSuccess("Lấy thông tin phòng chat thành công", result);
            }
            catch (Exception ex)
            {
                return _response.SetFail("Lỗi hệ thống: " + ex.Message);
            }
        }

        public async Task<ResponseMessageResult> DeleteMessageAsync(int messageId, Guid userId)
        {
            try
            {
                var message = await _context.ChatMessages
                    .FirstOrDefaultAsync(m => m.Id == messageId);

                if (message == null)
                    return _response.SetFail("Không tìm thấy tin nhắn", 404);

                // Chỉ người gửi mới có quyền xóa tin nhắn của chính họ
                if (message.SenderId != userId)
                    return _response.SetFail("Bạn không có quyền xóa tin nhắn này", 403);

                message.IsDeleted = true;
                // Tùy chọn: message.Content = "Tin nhắn đã bị thu hồi"; 

                await _context.SaveChangesAsync();
                return _response.SetSuccess("Thu hồi tin nhắn thành công");
            }
            catch (Exception ex)
            {
                return _response.SetFail("Lỗi khi xóa tin nhắn: " + ex.Message);
            }
        }
    }
}
