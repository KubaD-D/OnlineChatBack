namespace OnlineChatBack.Models
{
    public class Message
    {
        public required string Sender { get; set; }
        public required string TimeSent { get; set; }
        public required string Content { get; set; }
    }
}
