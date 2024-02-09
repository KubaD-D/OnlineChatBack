namespace OnlineChatBack.Models
{
    public class ChatRoom
    {
        public Guid Id { get; set; }
        public HashSet<string> Usernames { get; set; }
        public List<Message> Messages { get; set; }

        public ChatRoom()
        {
            Id = Guid.NewGuid();
            Usernames = new HashSet<string>();
            Messages = new List<Message>();

            // for testing purposes some dummie messages and user

            Usernames.Add("admin");

            Messages.Add(new Message {
                Sender = "Sender 1",
                Content = "This is a nice message"
            });

            Messages.Add(new Message
            {
                Sender = "Sender 2",
                Content = "dgsgsadgdds"
            });

            Messages.Add(new Message
            {
                Sender = "Sender 1",
                Content = "Great"
            });

            Messages.Add(new Message
            {
                Sender = "Sender 2",
                Content = "Yeah this is super great isn't it"
            });
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
