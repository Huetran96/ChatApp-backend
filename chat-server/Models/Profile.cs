using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace chat_server.Models
{
    public class Profile
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string? Gender { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public DateTime? Dob { get; set; }
        public string? Bio { get; set; } = string.Empty;
        public string? Hobby { get; set; } = string.Empty;
        public string? Sport { get; set; } = string.Empty;
        public string? School { get; set; } = string.Empty;
        public string? College { get; set; } = string.Empty;
        public string? WorkStatus { get; set; } = string.Empty;
        public string? Organization { get; set; } = string.Empty;
        public User User { get; set; }
    }
}
