using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace OnlineChatBack.Models
{
    public class ChatRoom
    {
        public Guid Id { get; set; }
        public string Owner { get; set; }   
        public List<string> Usernames { get; set; }
        [JsonIgnore]
        public List<Message> Messages { get; set; }

        public ChatRoom(string owner)
        {
            Id = Guid.NewGuid();
            Usernames = new List<string>();
            Messages = new List<Message>();
            Owner = owner;
            Usernames.Add(owner);
        }

    }
}
