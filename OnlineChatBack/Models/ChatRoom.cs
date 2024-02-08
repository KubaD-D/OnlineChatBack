namespace OnlineChatBack.Models
{
    public class ChatRoom
    {
        public Guid Id { get; set; }
        public List<User> Users { get; set; } = new();

        public void AddUser(User user)
        {
            Users.Add(user);
        }

        public bool ContainsUser(User user)
        {
            return Users.Contains(user);
        }
    }
}
