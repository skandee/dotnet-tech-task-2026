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
    }
}
