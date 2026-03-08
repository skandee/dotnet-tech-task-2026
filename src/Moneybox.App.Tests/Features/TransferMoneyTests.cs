using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;

namespace Moneybox.App.Tests.Features
{
    [TestFixture]
    public class TransferMoneyTests
    {
        private Mock<IAccountRepository> _mockAccountRepository;
        private Mock<INotificationService> _mockNotificationService;
        private TransferMoney _transferMoney;

        private Account _fromAccount;
        private Account _toAccount;

        private readonly Guid _fromAccountId = Guid.NewGuid();
        private const string _fromAccountEmail = "from@test.com";
        private const decimal _fromAccountBalance = 4000m;
        private const decimal _fromAccountTotalWithdrawn = 0m;
        private const decimal _fromAccountTotalPaidIn = 0m;


        private readonly Guid _toAccountId = Guid.NewGuid();
        private const string _toAccountemail = "to@test.com";
        private const decimal _toAccountBalance = 100m;
        private const decimal _toAccountTotalWithdrawn = 0m;
        private const decimal _toAccountTotalPaidIn = 0m;

        [SetUp]
        public void SetUp()
        {
            SetupData();

            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockNotificationService = new Mock<INotificationService>();

            _transferMoney = new TransferMoney(
                _mockAccountRepository.Object,
                _mockNotificationService.Object);

            _mockAccountRepository
               .Setup(r => r.GetAccountById(_fromAccountId))
               .Returns(_fromAccount);

            _mockAccountRepository
                .Setup(r => r.GetAccountById(_toAccountId))
                .Returns(_toAccount);
        }
        

        [TestCaseSource(nameof(GetInvalidInputData))]
        public void Execute_WhenInvalidInput_ThrowsArgumentException(Guid fromAccountId, Guid toAccountId, decimal amount, string expectedMessage)
        {
            // Act
            var ex = Assert.Throws<ArgumentException>(
                () => _transferMoney.Execute(fromAccountId, toAccountId, amount));

            // Assert
            Assert.That(ex.Message, Does.Contain(expectedMessage));

            _mockAccountRepository.Verify(
                r => r.GetAccountById(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        [TestCase(200)]
        public void Execute_ValidTransfer(decimal transferAmount)
        {
            // Act
            _transferMoney.Execute(_fromAccountId, _toAccountId, transferAmount);

            // Assert
            Assert.That(_fromAccount.Balance, Is.EqualTo(_fromAccountBalance - transferAmount));
            Assert.That(_fromAccount.Withdrawn, Is.EqualTo(_fromAccountTotalWithdrawn - transferAmount));

            Assert.That(_toAccount.Balance, Is.EqualTo(_toAccountBalance + transferAmount));
            Assert.That(_toAccount.PaidIn, Is.EqualTo(_toAccountTotalPaidIn + transferAmount));

            _mockAccountRepository.Verify(r => r.Update(_fromAccount), Times.Once);
            _mockAccountRepository.Verify(r => r.Update(_toAccount), Times.Once);
        }

        [Test]        
        public void Execute_ValidTransfer_DoesNotSendAnyNotifications()
        {
            // Arrange
            const decimal transferAmount = 200m;
            Guid fromAccountId =  Guid.NewGuid();
            Guid toAccountId = Guid.NewGuid();
            decimal fromAccountBalance = Account.LowFundsThreshold + transferAmount;
            decimal toPayIntotal = 3000;

            var fromAccount = Account.Create(
               _fromAccountId,
               new User
               {
                   Email = _fromAccountEmail
               },
               fromAccountBalance,
               _fromAccountTotalWithdrawn,
               _fromAccountTotalPaidIn);

            var toAccount = Account.Create(
                _toAccountId,
                new User
                {
                    Email = _toAccountemail
                },
                _toAccountBalance,
                _toAccountTotalWithdrawn,
                toPayIntotal);

            _mockAccountRepository
               .Setup(r => r.GetAccountById(fromAccountId))
               .Returns(fromAccount);

            _mockAccountRepository
                .Setup(r => r.GetAccountById(toAccountId))
                .Returns(toAccount);

            // Act
            _transferMoney.Execute(fromAccountId, toAccountId, transferAmount);

            // Verify
            _mockNotificationService.Verify(
                n => n.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _mockNotificationService.Verify(
                n => n.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Execute_ValidTransfer_SendNotifyFundsLowNotificationOnly()
        {
            // Arrange
            const decimal transferAmount = 200m;
            Guid fromAccountId = Guid.NewGuid();
            Guid toAccountId = Guid.NewGuid();

            var fromAccount = Account.Create(
               _fromAccountId,
               new User
               {
                   Email = _fromAccountEmail
               },
               transferAmount,
               _fromAccountTotalWithdrawn,
               _fromAccountTotalPaidIn);

            var toAccount = Account.Create(
                _toAccountId,
                new User
                {
                    Email = _toAccountemail
                },
                _toAccountBalance,
                _toAccountTotalWithdrawn,
                _toAccountTotalPaidIn);

            _mockAccountRepository
               .Setup(r => r.GetAccountById(fromAccountId))
               .Returns(fromAccount);

            _mockAccountRepository
                .Setup(r => r.GetAccountById(toAccountId))
                .Returns(toAccount);

            // Act
            _transferMoney.Execute(fromAccountId, toAccountId, transferAmount);

            // Verify
            _mockNotificationService.Verify(
                n => n.NotifyFundsLow(It.IsAny<string>()), Times.Once);
            _mockNotificationService.Verify(
                n => n.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Execute_ValidTransfer_SendNotifyApproachingPayInLimitNotificationOnly()
        {
            // Arrange
            const decimal transferAmount = 3600m;
            Guid fromAccountId = Guid.NewGuid();
            Guid toAccountId = Guid.NewGuid();
            decimal fromAccountBalance = Account.LowFundsThreshold + transferAmount;
            decimal toPayInTotal = 10;

            var fromAccount = Account.Create(
               _fromAccountId,
               new User
               {
                   Email = _fromAccountEmail
               },
               fromAccountBalance,
               _fromAccountTotalWithdrawn,
               _fromAccountTotalPaidIn);

            var toAccount = Account.Create(
                _toAccountId,
                new User
                {
                    Email = _toAccountemail
                },
                _toAccountBalance,
                _toAccountTotalWithdrawn,
                toPayInTotal);

            _mockAccountRepository
               .Setup(r => r.GetAccountById(fromAccountId))
               .Returns(fromAccount);

            _mockAccountRepository
                .Setup(r => r.GetAccountById(toAccountId))
                .Returns(toAccount);

            // Act
            _transferMoney.Execute(fromAccountId, toAccountId, transferAmount);

            // Verify
            _mockNotificationService.Verify(
                n => n.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _mockNotificationService.Verify(
                n => n.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Once);
        }
              
        [Test]
        public void Execute_ValidTransfer_SendNotifyFundsLowAndNotifyApproachingPayInLimitNotification()
        {
            // Arrange
            const decimal transferAmount = 3600m;
            Guid fromAccountId = Guid.NewGuid();
            Guid toAccountId = Guid.NewGuid();
            decimal toAccountBalance = Account.LowFundsThreshold + transferAmount - 10;

            var fromAccount = Account.Create(
               _fromAccountId,
               new User
               {
                   Email = _fromAccountEmail
               },
               transferAmount,
               _fromAccountTotalWithdrawn,
               _fromAccountTotalPaidIn);

            var toAccount = Account.Create(
                _toAccountId,
                new User
                {
                    Email = _toAccountemail
                },
                toAccountBalance,
                _toAccountTotalWithdrawn,
                _toAccountTotalPaidIn);

            _mockAccountRepository
               .Setup(r => r.GetAccountById(fromAccountId))
               .Returns(fromAccount);

            _mockAccountRepository
                .Setup(r => r.GetAccountById(toAccountId))
                .Returns(toAccount);

            // Act
            _transferMoney.Execute(fromAccountId, toAccountId, transferAmount);

            // Verify
            _mockNotificationService.Verify(
                n => n.NotifyFundsLow(It.IsAny<string>()), Times.Once);
            _mockNotificationService.Verify(
                n => n.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Once);
        }

        private void SetupData()
        {
            _fromAccount = Account.Create(
                _fromAccountId,
                new User
                {
                    Email = _fromAccountEmail
                },
                _fromAccountBalance,
                _fromAccountTotalWithdrawn,
                _fromAccountTotalPaidIn);

            _toAccount = Account.Create(
                _toAccountId,
                new User
                {
                    Email = _toAccountemail
                },
                _toAccountBalance,
                _toAccountTotalWithdrawn,
                _toAccountTotalPaidIn);
        }

        private static IEnumerable<TestCaseData> GetInvalidInputData()
        {
            var sameId = Guid.NewGuid();

            yield return new TestCaseData(Guid.Empty, Guid.NewGuid(), 10m, "From account id cannot be empty")
                .SetName("FromAccountId_Empty");

            yield return new TestCaseData(Guid.NewGuid(), Guid.Empty, 10m, "To account id cannot be empty")
                .SetName("ToAccountId_Empty");

            yield return new TestCaseData(sameId, sameId, 10m, "Cannot transfer money to the same account")
                .SetName("SameAccountId");

            yield return new TestCaseData(Guid.NewGuid(), Guid.NewGuid(), 0m, "Transfer amount must be greater than zero")
                .SetName("Amount_Zero");

            yield return new TestCaseData(Guid.NewGuid(), Guid.NewGuid(), -1m, "Transfer amount must be greater than zero")
                .SetName("Amount_Negative");
        }
    }
}

