using System;

namespace Moneybox.App.Domain.Exceptions
{
    public class PayInLimitExceededException : Exception
    {
        public PayInLimitExceededException(string message)
            : base(message) { }

        public PayInLimitExceededException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
