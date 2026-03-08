using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            ValidateInputs(fromAccountId, toAccountId, amount);

            var from = this.accountRepository.GetAccountById(fromAccountId);
            var to = this.accountRepository.GetAccountById(toAccountId);

            from.WithdrawFunds(amount);
            to.PayIn(amount);

            this.accountRepository.Update(from);
            this.accountRepository.Update(to);

            NotifyIfRequired(from, to);
        }

        /// <summary>
        /// Sends notifications to account holders if their account has crossed a threshold.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void NotifyIfRequired(Account from, Account to)
        {
            if (from.IsLowFunds)
            {
                this.notificationService.NotifyFundsLow(from.User.Email);
            }

            if (to.IsApproachingPayInLimit)
            {
                this.notificationService.NotifyApproachingPayInLimit(to.User.Email);
            }
        }

        /// <summary>
        /// Validates the inputs before executing a transfer.
        /// </summary>
        /// <param name="fromAccountId"></param>
        /// <param name="toAccountId"></param>
        /// <param name="amount"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidateInputs(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            if (fromAccountId == Guid.Empty)
            {
                throw new ArgumentException("From account id cannot be empty.", nameof(fromAccountId));
            }

            if (toAccountId == Guid.Empty)
            {
                throw new ArgumentException("To account id cannot be empty.", nameof(toAccountId));
            }

            if (fromAccountId == toAccountId)
            {
                throw new ArgumentException("Cannot transfer money to the same account.");
            }

            if (amount <= 0m)
            {
                throw new ArgumentException("Transfer amount must be greater than zero.", nameof(amount));
            }
        }
    }
}
