namespace chat_server.DTOs
{
    public sealed record SendMessageDto(
        Guid UserId,
        Guid ToUserId,
        string Content);
}

