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
                TimeSent = DateTime.Now.ToString("yyyy.MM.dd HH:mm"),
                Content = "This is a nice message"
            });

            Messages.Add(new Message
            {
                Sender = "Sender 2",
                TimeSent = DateTime.Now.ToString("yyyy.MM.dd HH:mm"),
                Content = "dgsgsadgdds"
            });

            Messages.Add(new Message
            {
                Sender = "Sender 1",
                TimeSent = DateTime.Now.ToString("yyyy.MM.dd HH:mm"),
                Content = "Great"
            });

            Messages.Add(new Message
            {
                Sender = "Sender 2",
                TimeSent = DateTime.Now.ToString("yyyy.MM.dd HH:mm"),
                Content = "Yeah this is super great isn't it"
            });

            for(int i = 0; i < 40; i++)
            {
                string sender = i % 2 == 0 ? "admin" : "string";

                Messages.Add(new Message
                {
                    Sender = sender,
                    TimeSent = DateTime.Now.ToString("yyyy.MM.dd HH:mm"),
                    Content = $"This is message number {i}"
                });
            }
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
