namespace chat_server.DTOs
{
    public class TokenReponseDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Role {  get; set; }
        public bool isValid { get; set; } = false;
        public string Message { get; set; }

        public string Token { get; set; }
        public DateTime? expGame {  get; set; }
        public DateTime exp {  get; set; }
        public DateTime now { get; set; } = DateTime.UtcNow;
    }
}
