using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace EduCore.API.Hubs
{
    public class ChatHub : Hub
    {
        // Khi client mở một phòng chat, họ sẽ gọi hàm này để gia nhập nhóm
        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        // Khi client đóng phòng chat
        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        }
    }
}
