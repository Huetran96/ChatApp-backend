using Microsoft.AspNetCore.Identity;

namespace chat_server.Models
{
    public class User : IdentityUser
    {
        public string Status {  get; set; }
        public string? Otp {  get; set; }

    }
}
