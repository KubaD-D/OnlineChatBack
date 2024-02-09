namespace OnlineChatBack.Models
{
    public class ChatRoom
    {
        public Guid Id { get; set; }
        public HashSet<User> Users { get; set; }

        public ChatRoom()
        {
            Id = Guid.NewGuid();
            Users = new HashSet<User>();
        }

        public void AddUser(User user)
        {
            Users.Add(user);
        }

        public bool ContainsUserWithUsername(string username)
        {
            foreach (var user in Users)
            {
                if (user.Username == username)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
