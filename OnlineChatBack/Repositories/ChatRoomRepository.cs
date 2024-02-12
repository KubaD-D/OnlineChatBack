using OnlineChatBack.Models;

namespace OnlineChatBack.Repositories
{
    public class ChatRoomRepository
    {
        public Dictionary<Guid, ChatRoom> ChatRooms { get; set; } = new();

        public bool AddChatRoom(ChatRoom chatRoom)
        {
            return ChatRooms.TryAdd(chatRoom.Id, chatRoom);
        }

        public ChatRoom? GetChatRoom(Guid id)
        {
            return ChatRooms.GetValueOrDefault(id);
        }

        public Dictionary<Guid, ChatRoom> GetChatRooms(User user)
        {
            var chatRooms = new Dictionary<Guid, ChatRoom>();

            foreach (var chatRoom in ChatRooms)
            {
                if (chatRoom.Value.Usernames.Contains(user.Username))
                {
                    chatRooms.TryAdd(chatRoom.Key, chatRoom.Value);
                }
            }

            return chatRooms;
        }
    }
}
