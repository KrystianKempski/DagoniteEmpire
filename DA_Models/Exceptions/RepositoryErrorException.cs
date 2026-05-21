namespace DagoniteEmpire.Exceptions
{
    public class RepositoryErrorException : Exception
    {
        public RepositoryErrorException(string message) : base(message)
        {
        }

        public RepositoryErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
