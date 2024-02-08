using OnlineChatBack.Models;

namespace OnlineChatBack
{
    public class ChatRoomRepository
    {
        public List<ChatRoom> ChatRooms { get; set; } = new();

        public void AddChatRoom(ChatRoom chatRoom)
        {
            ChatRooms.Add(chatRoom);
        }

        public ChatRoom? GetChatRoom(Guid id)
        {
            foreach (var chatRoom in ChatRooms)
            {
                if (chatRoom.Id == id)
                {
                    return chatRoom;
                }
            }

            return null;
        }

        public List<ChatRoom> GetChatRooms(User user) 
        {
            var chatRooms = new List<ChatRoom>();

            foreach(var chatRoom in ChatRooms)
            {
                if(chatRoom.Users.Contains(user))
                {
                    chatRooms.Add(chatRoom);
                }
            }

            return chatRooms;
        }
    }
}
