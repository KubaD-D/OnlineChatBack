using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace OnlineChatBack.Models
{
    public class Message
    {
        [JsonIgnore]
        public int Id { get; set; }
        public required string Sender { get; set; }
        public required DateTime TimeSent { get; set; }
        public required string Content { get; set; }
        [JsonIgnore]
        public required Guid ChatRoomId { get; set; }
        [JsonIgnore]
        public required ChatRoom ChatRoom { get; set; }
    }
}
