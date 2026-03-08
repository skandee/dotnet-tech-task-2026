using Moneybox.App.Domain.Exceptions;
using NUnit.Framework;

namespace Moneybox.App.Tests.Domain
{
    [TestFixture]
    public class AccountTests
    {
        private readonly Guid _accountId = Guid.NewGuid();
        private readonly User _user = new User { Email = "from@test.com" };

        [TestCaseSource(nameof(GetBalanceTestData))]
        public void IsLowFunds_ReturnTrueOrFalse(decimal balance, bool expectedResult)
        {
            // Act
            var account = Account.Create(_accountId, _user, balance, 0m, 0m);

            // Assert
            Assert.That(account.IsLowFunds, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCaseSource(nameof(GetPaidInTestData))]
        public void IsApproachingPayInLimit_ReturnTrueOrFalse(decimal paidIn, bool expectedResult)
        {           
            // Act
            var account = Account.Create(_accountId, _user, 1000m, 0m, paidIn);

            // Assert
            Assert.That(account.IsApproachingPayInLimit, Is.EqualTo(expectedResult));
        }
        
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        public void WithdrawFunds_InvalidAmount_ThrowsInvalidAmountException(decimal amount)
        {
            // Arrange 
            var account = Account.Create(_accountId, _user, 1000m, 0m, 0m);

            // Act
            Assert.Throws<InvalidAmountException>(
                () => account.WithdrawFunds(amount));
        }

        [Test]
        public void WithdrawFunds_AmountExceedsBalance_ThrowsInsufficientFundsException()
        {
            // Arrange 
            var balance = 1000m;
            var withdrawAmount = 1001m;
            var account = Account.Create(_accountId, _user, balance, 0m, 0m);

            // Act
            Assert.Throws<InsufficientFundsException>(
                () => account.WithdrawFunds(withdrawAmount));
        }

        [Test]
        [TestCase(1000, 200)]
        [TestCase(1000, 1000)]       
        public void WithdrawFunds_ValidAmount(decimal balance, decimal withdrawAmount)
        {
            // Arrange 
            var withdrawn = 10m;
            var account = Account.Create(_accountId, _user, balance, withdrawn, 10m);

            // Act
            account.WithdrawFunds(withdrawAmount);

            // Assert
            Assert.That(account.Balance, Is.EqualTo(balance - withdrawAmount));
            Assert.That(account.Withdrawn, Is.EqualTo(-withdrawAmount + withdrawn));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        public void PayIn_InvalidAmount_ThrowsInvalidAmountException(decimal amount)
        {
            // Arrange 
            var account = Account.Create(_accountId, _user, 1000m, 0m, 0m);

            // Act
            var ex =  Assert.Throws<InvalidAmountException>(
                () => account.PayIn(amount));

            // Assert
            Assert.That(ex.Message, Does.Contain("Amount must be greater than zero"));
        }

        [Test]
        public void PayIn_ValidAmount_IncreasesBalanceAndPaidIn()
        {
            // Arrange 
            var balance = 1000m;
            var withdrawAmount = 200m;
            var account = Account.Create(_accountId, _user, balance, 0m, 0m);

            // Act
            account.PayIn(withdrawAmount);

            // Assert
            Assert.That(account.Balance, Is.EqualTo(1200m));
            Assert.That(account.PaidIn, Is.EqualTo(200m));
        }

        [Test]
        public void PayIn_ExceedsPayInLimit_ThrowsPayInLimitExceededException()
        {
            // Arrange 
            var paidIn = Account.PayInLimit - 100m;
            var balance = 1000m;
            var paidInAmount = 200m;
            var account = Account.Create(_accountId, _user, balance, 0m, paidIn);

            // Act
            Assert.Throws<PayInLimitExceededException>(
                () => account.PayIn(paidInAmount));           
        }

        [Test]
        public void Create_WithValidData_SetsCorrectValues()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            const string email = "from@test.com";
            const decimal balance = 4000m;
            const decimal withdrawn = 10m;
            const decimal paidIn = 20m;

            // Act
            var account = Account.Create(
                   id,
                    new User
                    {
                        Email = email
                    },
                    balance,
                    withdrawn,
                    paidIn);

            // Assert
            Assert.That(account.Id, Is.EqualTo(id));
            Assert.That(account.Balance, Is.EqualTo(balance));
            Assert.That(account.Withdrawn, Is.EqualTo(withdrawn));
            Assert.That(account.PaidIn, Is.EqualTo(paidIn));
            Assert.That(account.User.Email, Is.EqualTo(email));
        }

        private static IEnumerable<TestCaseData> GetBalanceTestData()
        {
            yield return new TestCaseData(Account.LowFundsThreshold - 1m, true)
                .SetName("IsLowFunds_BalanceBelowThreshold");

            yield return new TestCaseData(Account.LowFundsThreshold, false)
                .SetName("IsLowFunds_BalanceExactlyAtThreshold");

            yield return new TestCaseData(Account.LowFundsThreshold + 1m, false)
                .SetName("IsLowFunds_BalanceAboveThreshold");
        }

        private static IEnumerable<TestCaseData> GetPaidInTestData()
        {
            yield return new TestCaseData(Account.PayInLimit - (Account.LowFundsThreshold - 1m), true)
                .SetName("IsApproachingPayInLimit_RemainingPaidInBelowThreshold");

            yield return new TestCaseData(Account.PayInLimit - Account.LowFundsThreshold, false)
                .SetName("IsApproachingPayInLimit_RemainingPaidInExactlyAtThreshold");

            yield return new TestCaseData(Account.PayInLimit - (Account.LowFundsThreshold + 1m), false)
                .SetName("IsApproachingPayInLimit_RemainingPaidInAboveThreshold");
        }
    }
}
