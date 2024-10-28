using chat_server.Models;

namespace chat_server.DTOs
{
    public class ListUserDto
    {
        public int totalPage { get; set; } 
        public List<User> users { get; set; }
    }
}
