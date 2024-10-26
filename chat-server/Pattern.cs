namespace chat_server
{
    public class Pattern
    {
        public const string EMAIL_REGEX = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public const string PHONE_REGEX = @"^[A-z][A-z0-9]{3,23}";
    }
}
