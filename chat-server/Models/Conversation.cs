using System.ComponentModel.DataAnnotations;

namespace chat_server.Models
{
    public class Conversation
    {
        [Key]
        public string ConversationId { get; set; } = Guid.NewGuid().ToString();
        public string ConversationName { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public string CreateBy { get; set; }

        public List<Message> Messages { get; set; }
    }
}
