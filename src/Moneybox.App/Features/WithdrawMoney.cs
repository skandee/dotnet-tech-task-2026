using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            ValidateInputs(fromAccountId, amount);

            var from = this.accountRepository.GetAccountById(fromAccountId);

            from.WithdrawFunds(amount);

            this.accountRepository.Update(from);

            NotifyIfRequired(from);
        }

        /// <summary>
        /// Notifies the account holder if their balance has fallen below the low funds threshold.
        /// </summary>
        /// <param name="from"></param>
        private void NotifyIfRequired(Account from)
        {
            if (from.IsLowFunds)
            {
                this.notificationService.NotifyFundsLow(from.User.Email);
            }
        }

        /// <summary>
        /// Validates the inputs before executing a transfer.
        /// </summary>
        /// <param name="fromAccountId"></param>
        /// <param name="amount"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidateInputs(Guid fromAccountId, decimal amount)
        {
            if (fromAccountId == Guid.Empty)
            {
                throw new ArgumentException("From account id cannot be empty.", nameof(fromAccountId));
            }

            if (amount <= 0m)
            {
                throw new ArgumentException("Withdraw amount must be greater than zero.", nameof(amount));
            }
        }
    }
}
