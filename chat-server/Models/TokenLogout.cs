namespace chat_server.Models
{
    public class TokenLogout
    {
        public string Id { get; set; }
        public DateTime expireDate { get; set; }
    }
}
