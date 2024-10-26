using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace chat_server.Models
{
    public class Friendship
    {
        [Key]

        public string RequestedId { get; set; }
        public User RequestedUser { get; set; }
        public string AcceptedId { get; set; }
        public User AppceptedUser { get; set; }
        public string Status { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
    }
}
