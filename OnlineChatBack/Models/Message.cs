using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace OnlineChatBack.Models
{
    public class Message
    {
        [JsonIgnore]
        public int Id { get; set; }
        public required string Sender { get; set; }
        public DateTime TimeSent { get; set; }
        public required string Content { get; set; }
    }
}
