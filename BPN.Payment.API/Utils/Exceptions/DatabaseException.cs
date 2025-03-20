namespace BPN.Payment.API.Utils.Exceptions
{
    public class DatabaseException : Exception
    {
        public DatabaseException()
            : base("Database error occurred. Please try again later.") { }
}
}
