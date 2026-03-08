using System;

namespace Moneybox.App.Domain.Exceptions
{
    public class InvalidAmountException : Exception
    {
        public InvalidAmountException(string message)
            : base(message) { }

        public InvalidAmountException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
