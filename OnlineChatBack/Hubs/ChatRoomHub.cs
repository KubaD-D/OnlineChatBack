using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineChatBack.Dtos;
using OnlineChatBack.Models;

namespace OnlineChatBack.Hubs
{

    [Authorize]
    public class ChatRoomHub : Hub
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public ChatRoomHub(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task JoinRoom(Guid chatRoomId)
        {
            var username = Context.User?.Identity?.Name;

            if(username == null)
            {
                return;
            }

            var chatRoom = await _applicationDbContext.ChatRooms.FindAsync(chatRoomId);

            if(chatRoom == null || !chatRoom.Usernames.Contains(username))
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoom.Id.ToString());
            await Clients.Group(chatRoomId.ToString()).SendAsync("UserJoined", username, Context.ConnectionId, chatRoomId);
        }

        public async Task LeaveRoom(Guid chatRoomId)
        {
            var username = Context.User?.Identity?.Name;

            if (username == null)
            {
                return;
            }

            var chatRoom = await _applicationDbContext.ChatRooms.FindAsync(chatRoomId);

            if (chatRoom == null || !chatRoom.Usernames.Contains(username))
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId.ToString());
        }

        public async Task SendMessage(Guid chatRoomId, string message)
        {
            var username = Context.User?.Identity?.Name;

            if (username == null)
            {
                return;
            }

            var chatRoom = await _applicationDbContext.ChatRooms.FindAsync(chatRoomId);

            if (chatRoom == null || !chatRoom.Usernames.Contains(username))
            {
                return;
            }

            var timeSent = DateTime.Now.ToString("yyyy.MM.dd HH:mm");

            await Clients.Group(chatRoomId.ToString()).SendAsync("ReceiveMessage", new { Sender = username, TimeSent = timeSent, Content = message });
        }
    }
}
