using Moneybox.App.Domain.Exceptions;
using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;
        public const decimal LowFundsThreshold = 500m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        public bool IsLowFunds => Balance < LowFundsThreshold;

        public bool IsApproachingPayInLimit => PayInLimit - PaidIn < LowFundsThreshold;

        /// <summary>
        /// Withdraw funds from this account.
        /// </summary>
        /// <param name="amount"></param>
        /// <exception cref="InvalidAmountException">Thrown when amount is zero or negative.</exception>
        /// <exception cref="InsufficientFundsException">Thrown when balance would go below zero.</exception>
        public void WithdrawFunds(decimal withdrawAmount)
        {
            if (withdrawAmount <= 0m)
            {
                throw new InvalidAmountException($"Amount must be greater than zero. WithdrawAmount: {withdrawAmount}");
            }

            if (Balance < withdrawAmount)
            {
                throw new InsufficientFundsException("Insufficient funds to withdrow");
            }

            Balance -= withdrawAmount;
            Withdrawn -= withdrawAmount;
        }

        /// <summary>
        /// Pay funds into this account.
        /// </summary>
        /// <exception cref="InvalidAmountException">Thrown when amount is zero or negative.</exception>
        /// <exception cref="PayInLimitExceededException">Thrown when pay-in limit would be exceeded.</exception>
        public void PayIn(decimal amount)
        {
            if (amount <= 0m)
            {
                throw new InvalidAmountException($"Amount must be greater than zero. PayIn Amount: {amount}");
            }

            if (PaidIn + amount > PayInLimit)
            {
                throw new PayInLimitExceededException("Account pay in limit reached");
            }

            Balance += amount;
            PaidIn += amount;
        }
    }
}
