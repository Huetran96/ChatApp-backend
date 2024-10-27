namespace chat_server.DTOs
{
    public class ResponeGameDto
    {
        public bool isValid { get; set; }
        public DateTime? exp {  get; set; }
        public DateTime now { get; set; } = DateTime.Now;
    }
}
