namespace chat_server.Helpers
{
    public class QueryObject
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Keyword { get; set; } =string.Empty;
    }
}
