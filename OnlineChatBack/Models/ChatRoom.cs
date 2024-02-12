using System.Reflection.Metadata.Ecma335;

namespace OnlineChatBack.Models
{
    public class ChatRoom
    {
        public Guid Id { get; set; }
        public string Owner { get; set; }   
        public HashSet<string> Usernames { get; set; }
        public List<Message> Messages { get; set; }

        public ChatRoom(string owner)
        {
            Id = Guid.NewGuid();
            Usernames = new HashSet<string>();
            Messages = new List<Message>();
            Owner = owner;
            Usernames.Add(owner);
        }

        public void AddUsername(string username)
        {
            Usernames.Add(username);
        }

        public bool ContainsUserWithUsername(string username)
        {
            foreach (var _username in Usernames)
            {
                if (_username == username)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
