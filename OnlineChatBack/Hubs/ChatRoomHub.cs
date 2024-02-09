using Microsoft.AspNetCore.SignalR;
using OnlineChatBack.Repositories;

namespace OnlineChatBack.Hubs
{
    public class UserConnection
    {
        public required string Username { get; set; }
        public Guid ChatRoomId { get; set; }
    }

    public class ChatRoomHub : Hub
    {
        private readonly ChatRoomRepository _chatRoomRepository;
        public ChatRoomHub(ChatRoomRepository chatRoomRepository)
        {
            _chatRoomRepository = chatRoomRepository;
        }

        public async Task JoinRoom(UserConnection userConnection)
        {
            var chatRoom = _chatRoomRepository.GetChatRoom(userConnection.ChatRoomId);

            if(chatRoom == null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ChatRoomNotFound", userConnection.ChatRoomId);
            }
            else if(!chatRoom.ContainsUserWithUsername(userConnection.Username))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ChatRoomUnauthorizedUser", userConnection.ChatRoomId);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.ChatRoomId.ToString());
            await Clients.Client(Context.ConnectionId).SendAsync("ChatRoomJoined", userConnection.ChatRoomId);
        }
    }
}
