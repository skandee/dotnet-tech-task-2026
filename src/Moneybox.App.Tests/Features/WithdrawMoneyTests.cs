using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;

namespace Moneybox.App.Tests.Features
{
    [TestFixture]
    public class WithdrawMoneyTests
    {
        private Mock<IAccountRepository> _mockAccountRepository;
        private Mock<INotificationService> _mockNotificationService;
        private WithdrawMoney _withdrawMoney;

        private Account _fromAccount;

        private readonly Guid _fromAccountId = Guid.NewGuid();
        private const string _fromAccountEmail = "from@test.com";
        private const decimal _fromAccountBalance = 4000m;
        private const decimal _fromAccountTotalWithdrawn = 0m;
        private const decimal _fromAccountTotalPaidIn = 0m;

        [SetUp]
        public void SetUp()
        {
            SetupData();

            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockNotificationService = new Mock<INotificationService>();

            _withdrawMoney = new WithdrawMoney(
                _mockAccountRepository.Object,
                _mockNotificationService.Object);

            _mockAccountRepository
               .Setup(r => r.GetAccountById(_fromAccountId))
               .Returns(_fromAccount);           
        }


        [TestCaseSource(nameof(GetInvalidInputData))]
        public void Execute_WhenInvalidInput_ThrowsArgumentException(Guid fromAccountId, decimal amount, string expectedMessage)
        {
            // Act
            var ex = Assert.Throws<ArgumentException>(
                () => _withdrawMoney.Execute(fromAccountId, amount));

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
            _withdrawMoney.Execute(_fromAccountId, transferAmount);

            // Assert
            Assert.That(_fromAccount.Balance, Is.EqualTo(_fromAccountBalance - transferAmount));
            Assert.That(_fromAccount.Withdrawn, Is.EqualTo(_fromAccountTotalWithdrawn - transferAmount));
                       
            _mockAccountRepository.Verify(r => r.Update(_fromAccount), Times.Once);
        }

        [Test]
        public void Execute_ValidTransfer_DoesNotSendAnyNotification()
        {
            // Arrange
            const decimal transferAmount = 200m;
            Guid fromAccountId = Guid.NewGuid();
            Guid toAccountId = Guid.NewGuid();
            decimal fromAccountBalance = Account.LowFundsThreshold + transferAmount;

            var fromAccount = Account.Create(
               _fromAccountId,
               new User
               {
                   Email = _fromAccountEmail
               },
               fromAccountBalance,
               _fromAccountTotalWithdrawn,
               _fromAccountTotalPaidIn);
            
            _mockAccountRepository
               .Setup(r => r.GetAccountById(fromAccountId))
               .Returns(fromAccount);
            
            // Act
            _withdrawMoney.Execute(fromAccountId, transferAmount);

            // Verify
            _mockNotificationService.Verify(
                n => n.NotifyFundsLow(It.IsAny<string>()), Times.Never);            
        }

        [Test]
        public void Execute_ValidTransfer_SendNotifyFundsLowNotification()
        {
            // Arrange
            const decimal transferAmount = 200m;
            Guid fromAccountId = Guid.NewGuid();
            Guid toAccountId = Guid.NewGuid();
            decimal fromAccountBalance = Account.LowFundsThreshold + transferAmount - 1;

            var fromAccount = Account.Create(
               _fromAccountId,
               new User
               {
                   Email = _fromAccountEmail
               },
               fromAccountBalance,
               _fromAccountTotalWithdrawn,
               _fromAccountTotalPaidIn);

            _mockAccountRepository
               .Setup(r => r.GetAccountById(fromAccountId))
               .Returns(fromAccount);

            // Act
            _withdrawMoney.Execute(fromAccountId, transferAmount);

            // Verify
            _mockNotificationService.Verify(
                n => n.NotifyFundsLow(It.IsAny<string>()), Times.Once);           
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
        }

        private static IEnumerable<TestCaseData> GetInvalidInputData()
        {
            yield return new TestCaseData(Guid.Empty, 10m, "From account id cannot be empty")
                .SetName("FromAccountId_Empty");

            yield return new TestCaseData(Guid.NewGuid(), 0m, "Withdraw amount must be greater than zero")
                .SetName("Amount_Zero");

            yield return new TestCaseData(Guid.NewGuid(), -1m, "Withdraw amount must be greater than zero")
                .SetName("Amount_Negative");
        }
    }
}
