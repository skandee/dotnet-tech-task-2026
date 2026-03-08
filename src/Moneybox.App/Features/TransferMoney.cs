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
    }
}
