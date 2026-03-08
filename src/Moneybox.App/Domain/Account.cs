using Moneybox.App.Domain.Exceptions;
using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;
        public const decimal LowFundsThreshold = 500m;

        public Guid Id { get; private set; }

        public User User { get; private set; }

        public decimal Balance { get; private set; }

        public decimal Withdrawn { get; private set; }

        public decimal PaidIn { get; private set; }

        private Account() { }

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
                    

        /// <summary>
        /// Factory method to create a new Account instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <param name="balance"></param>
        /// <param name="withdrawn"></param>
        /// <param name="paidIn"></param>
        /// <returns></returns>
        public static Account Create(Guid id,
            User user,
            decimal balance,
            decimal withdrawn,
            decimal paidIn)
        {
            return new Account
            {
                Id = id,
                User = user,
                Balance = balance,
                Withdrawn = withdrawn,
                PaidIn = paidIn
            };
        }
    }
}
