using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;
using ATM.Domain.Interfaces;
using ATM.Domain.Interfaces.Services;
using ATM.Domain.Interfaces.Strategies;
using ATM.Infrastructure.Data;
using ATM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;


namespace ATM.UnitTests.Services
{
    public class AtmServicesTests
    {
        private readonly Mock<ICardRepository> _cardRepoMock = new();
        private readonly Mock<IAccountRepository> _accountRepoMock = new();
        private readonly Mock<ITransactionRepository> _transactionRepoMock = new();
        private readonly Mock<IAtmCassetteRepository> _cassettesRepoMock = new();
        private readonly Mock<IAtmOperationLogRepository> _operationLogRepoMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherRepoMock = new();
        private readonly Mock<ICashWithdrawalStrategy> _strategyRepoMock = new();

        private readonly AppDbContext _context;
        private readonly AtmService _service;

        public AtmServicesTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new AppDbContext(options);
            _service = new AtmService(
                _context,
                _cardRepoMock.Object,
                _accountRepoMock.Object,
                _transactionRepoMock.Object,
                _cassettesRepoMock.Object,
                _operationLogRepoMock.Object,
                _passwordHasherRepoMock.Object,
                _strategyRepoMock.Object
            );
        }

        [Fact]
        public async Task WithdrawAsync_ShouldDecreaseBalance_WhenDataIsValid()
        {
            var cardId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var pin = "1234";
            decimal initialBalance = 1000m;
            decimal withdrawAmount = 400m;

            var card = new Card { Id = cardId, AccountId = accountId, PinHash = "hashed_pin", IsBlocked = false };
            var account = new Account { Id = accountId, Balance = initialBalance };
            var cassettes = new List<AtmCassette> {  new AtmCassette { Denomination = 200, Count = 10  } };

            _cardRepoMock.Setup(r => r.GetByCardByIdAsync(cardId)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword(pin, card.PinHash)).Returns(true);
            _accountRepoMock.Setup(r => r.GetAccountByIdAsync(accountId)).ReturnsAsync(account);
            _cassettesRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cassettes);

            _strategyRepoMock.Setup(s => s.CalculateNotes(withdrawAmount, cassettes))
                .Returns(new Dictionary<AtmCassette, int> { { cassettes[0], 2 } });

            await _service.WithdrawCashAsync(cardId, pin, withdrawAmount);

            Assert.Equal(600m, account.Balance);

            _accountRepoMock.Verify(r => r.UpdateAsync(It.Is<Account>(a => a.Balance == 600m)), Times.Once);
            _transactionRepoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task WithdrawAsync_ShouldThrowException_WhenPinIsWrong()
        {
            var cardId = Guid.NewGuid();
            var card = new Card { Id =  cardId, PinHash = "correct_hash" };

            _cardRepoMock.Setup(r => r.GetByCardByIdAsync(cardId)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword("wrong_pin", card.PinHash)).Returns(false);

            await Assert.ThrowsAsync<Exception>(() => _service.WithdrawCashAsync(cardId, "wrong_pin", 100));
        }

        [Fact]
        public async Task WithdrawAsync_ShouldThrowException_WhenCardIsBlocked()
        {
            var cardId = Guid.NewGuid();
            var card = new Card { Id = cardId, PinHash = "hash", IsBlocked = true };

            _cardRepoMock.Setup(r => r.GetByCardByIdAsync(cardId)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword(It.IsAny<string>(), card.PinHash)).Returns(true);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.WithdrawCashAsync(cardId, "1234", 100));
            Assert.Equal("Картку заблоковано через перевищення спроб введення ПІН-коду.", ex.Message);
        }

        [Fact]
        public async Task WithdrawAsync_ShouldThrowException_WhenInsufficientFunds()
        {
            var cardId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var card = new Card { Id = cardId, AccountId = accountId, PinHash = "hash" };
            var account = new Account { Id = accountId, Balance = 50m }; 

            _cardRepoMock.Setup(r => r.GetByCardByIdAsync(cardId)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword(It.IsAny<string>(), card.PinHash)).Returns(true);
            _accountRepoMock.Setup(r => r.GetAccountByIdAsync(accountId)).ReturnsAsync(account);

            
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.WithdrawCashAsync(cardId, "1234", 1000));
            Assert.Equal("Недостатньо коштів на рахунку", ex.Message);

            
        }

        [Fact]
        public async Task WithdrawAsync_ShouldThrowException_WhenAtmOutOfCash()
        {
            var cardId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var card = new Card { Id = cardId, AccountId = accountId, PinHash = "hash" };
            var account = new Account { Id = accountId, Balance = 1000m };
            var cassettes = new List<AtmCassette>(); 

            _cardRepoMock.Setup(r => r.GetByCardByIdAsync(cardId)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword(It.IsAny<string>(), card.PinHash)).Returns(true);
            _accountRepoMock.Setup(r => r.GetAccountByIdAsync(accountId)).ReturnsAsync(account);
            _cassettesRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cassettes);

            _strategyRepoMock.Setup(s => s.CalculateNotes(It.IsAny<decimal>(), cassettes))
                         .Throws(new Exception("Недостатньо коштів в банкоматі"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.WithdrawCashAsync(cardId, "1234", 500));
            Assert.Equal("Недостатньо коштів в банкоматі", ex.Message);
        }

        [Fact]
        public async Task GetBalanceAsync_ShouldReturnCorrectBalance_WhenPinIsValid()
        {
            var cardId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var card = new Card { Id = cardId, AccountId = accountId, PinHash = "hashed_pin" };
            var account = new Account { Id = accountId, Balance = 1234.56m };

            _cardRepoMock.Setup(r => r.GetByCardByIdAsync(cardId)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword("1234", "hashed_pin")).Returns(true);
            _accountRepoMock.Setup(r => r.GetAccountByIdAsync(accountId)).ReturnsAsync(account);

            var result = await _service.GetBalanceAsync(cardId, "1234");

            Assert.Equal(1234.56m, result);
        }

        [Fact]
        public async Task DepositAsync_ShouldIncreaseBalance_AndCreateTransaction()
        {
            var cardId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var card = new Card { Id = cardId, AccountId = accountId, PinHash = "hashed_pin" };
            var account = new Account { Id = accountId, Balance = 100m };
            var depositedBanknotes = new Dictionary<int, int>
            {
                { 200, 2 },
                { 100, 1 }
            };
            decimal expectedDepositAmount = 500m;

            var cassettes = new List<AtmCassette>
            {
                new AtmCassette { Denomination = 200, Count = 10 },
                new AtmCassette { Denomination = 100, Count = 10 }
            };
            _cassettesRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cassettes);

            _cardRepoMock.Setup(r => r.GetByCardByIdAsync(cardId)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword("1234", "hashed_pin")).Returns(true);
            _accountRepoMock.Setup(r => r.GetAccountByIdAsync(accountId)).ReturnsAsync(account);

            await _service.DepositCashAsync(cardId,  depositedBanknotes, "1234");

            Assert.Equal(600m, account.Balance); 
            _accountRepoMock.Verify(r => r.UpdateAsync(account), Times.Once);
            _transactionRepoMock.Verify(r => r.AddAsync(It.Is<Transaction>(t => t.Amount == expectedDepositAmount)), Times.Once);

            _cassettesRepoMock.Verify(r => r.UpdateAsync(It.IsAny<AtmCassette>()), Times.Exactly(2));
        }





        [Fact]
        public async Task DepositAsync_ShouldThrowException_WhenDepositAmountIsZero()
        {
            var cardId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var card = new Card { Id = cardId, AccountId = accountId, PinHash = "hash" };
            var account = new Account { Id = accountId, Balance = 100m };

            var depositedBanknotes = new Dictionary<int, int>();

            _cardRepoMock.Setup(r => r.GetByCardByIdAsync(cardId)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword("1234", "hash")).Returns(true);
            _accountRepoMock.Setup(r => r.GetAccountByIdAsync(accountId)).ReturnsAsync(account);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.DepositCashAsync(cardId, depositedBanknotes, "1234"));
            Assert.Equal("Сума поповнення має бути більшою за нуль.", ex.Message);
        }

        [Fact]
        public async Task DepositAsync_ShouldThrowException_WhenDenominationIsUnsupported()
        {
            var cardId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var card = new Card { Id = cardId, AccountId = accountId, PinHash = "hash" };
            var account = new Account { Id = accountId, Balance = 100m };

            var depositedBanknotes = new Dictionary<int, int> { { 300, 1 } };

            var cassettes = new List<AtmCassette>
            {
                new AtmCassette { Denomination = 200, Count = 10 },
                new AtmCassette { Denomination = 100, Count = 10 }
            };

            _cardRepoMock.Setup(r => r.GetByCardByIdAsync(cardId)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword("1234", "hash")).Returns(true);
            _accountRepoMock.Setup(r => r.GetAccountByIdAsync(accountId)).ReturnsAsync(account);
            _cassettesRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cassettes);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.DepositCashAsync(cardId, depositedBanknotes, "1234"));
            Assert.Equal("Банкомат не приймає банкноти такого номіналу 300", ex.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldBlockCard_AfterThreeFailedAttempts()
        {
            var cardNumber = "1111222233334444";
            var card = new Card { CardNumber = cardNumber, PinHash = "hash", FailedAttempts = 2, IsBlocked = false };

            _cardRepoMock.Setup(r => r.GetByCardByNumberAsync(cardNumber)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword("wrong_pin", "hash")).Returns(false); 

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AuthenticateAsync(cardNumber, "wrong_pin"));

            Assert.Equal("Картку заблоковано через перевищення спроб введення ПІН-коду.", ex.Message);
            Assert.True(card.IsBlocked);
            _cardRepoMock.Verify(r => r.UpdateAsync(It.Is<Card>(c => c.IsBlocked == true)), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldResetFailedAttempts_WhenLoginIsSuccessful()
        {
            var cardNumber = "1111222233334444";
            var card = new Card { CardNumber = cardNumber, PinHash = "hash", FailedAttempts = 1, IsBlocked = false };

            _cardRepoMock.Setup(r => r.GetByCardByNumberAsync(cardNumber)).ReturnsAsync(card);
            _passwordHasherRepoMock.Setup(h => h.VerifyPassword("correct_pin", "hash")).Returns(true); 
            var result = await _service.AuthenticateAsync(cardNumber, "correct_pin");

            Assert.True(result); 
            Assert.Equal(0, card.FailedAttempts); 
            _cardRepoMock.Verify(r => r.UpdateAsync(It.Is<Card>(c => c.FailedAttempts == 0)), Times.Once);
        }
    }
}
