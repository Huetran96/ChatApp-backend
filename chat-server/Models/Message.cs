using System.ComponentModel.DataAnnotations;

namespace chat_server.Models
{
    public class Message
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ConversationId { get; set; }
        public string SenderId { get; set; }
        public string MessageType { get; set; }
        public string Content { get; set; }
        public DateTime SendAt { get; set; } = DateTime.Now;
        public Conversation Conversation { get; set; }
        public User User { get; set; }

    }
}
