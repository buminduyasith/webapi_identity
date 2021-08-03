using System;
using webapi_identity.Domains;

namespace webapi_identity.Exceptions
{
    public abstract class DomainException : Exception
    {
        public DomainException(String message) : base(message)
        {

        }
    }

    public class UserAlreadyExistsException : DomainException
    {
        public Exception CustomeException { get; }
        public UserAlreadyExistsException(string message) : base(message)
        {

        }
    }

    public class UserNotFoundException : DomainException
    {
        public Exception CustomeException { get; }
        public UserNotFoundException(string message) : base(message)
        {

        }
    }

    public class InvalidUserException : DomainException
    {

        public InvalidUserException(string message = "User email or password invalid") : base(message)
        {

        }
    }
}