namespace chat_server.Helpers
{
    public class Pattern
    {
        public const string EMAIL_REGEX = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public const string PHONE_REGEX = @"^[A-z][A-z0-9]{3,23}";
    }
}
