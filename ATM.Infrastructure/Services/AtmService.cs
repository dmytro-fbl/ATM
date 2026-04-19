using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;
using ATM.Domain.Interfaces;
using ATM.Domain.Interfaces.Base;
using ATM.Domain.Interfaces.Services;
using ATM.Domain.Interfaces.Strategies;
using ATM.Infrastructure.Data;
using ATM.Infrastructure.Repositories;

namespace ATM.Infrastructure.Services
{
    public class AtmService : BaseService, IAtmService
    {
        private readonly AppDbContext _context;
        private ICardRepository _cardRepo;
        private IAccountRepository _accountRepo;
        private ITransactionRepository _transactionRepo;
        private IAtmCassetteRepository _cassetteRepo;
        private IAtmOperationLogRepository _operationLogRepo;
        private readonly IPasswordHasher _passwordHasher;

        private readonly ICashWithdrawalStrategy _withdrawalStrategy;

        public AtmService(AppDbContext context, ICardRepository cardRepo, IAccountRepository accountRepo,
            ITransactionRepository transactionRepo, IAtmCassetteRepository cassetteRepo,
            IAtmOperationLogRepository operationLogRepo, IPasswordHasher passwordHasher,
            ICashWithdrawalStrategy withdrawalStrategy
            ) : base(operationLogRepo)
        {
            _context = context;
            _cardRepo = cardRepo;
            _accountRepo = accountRepo;
            _transactionRepo = transactionRepo;
            _cassetteRepo = cassetteRepo;
            _passwordHasher = passwordHasher;
            _withdrawalStrategy = withdrawalStrategy;
        }



        public async Task<bool> AuthenticateAsync(string cardNumber, string pin)
        {
            var card = await _cardRepo.GetByCardByNumberAsync(cardNumber);

            if (card == null)
            {
                await LogWarningAsync("Невірний номер карти");
                throw new Exception("Картку з таким номером не знайдено");

            }

            if (card.IsBlocked)
            {
                await LogWarningAsync($"Карта {cardNumber} - заблокована");
                throw new Exception("Ваша карта заблокована");
            }
                        
            if (!_passwordHasher.VerifyPassword(pin, card.PinHash))
            {
                card.FailedAttempts++;
                if (card.FailedAttempts >= 3)
                {
                    card.IsBlocked = true;
                    await _cardRepo.UpdateAsync(card);
                    await LogWarningAsync("Картку заблоковано після 3 невдалих спроб", card.Id);
                    throw new Exception("Картку заблоковано через перевищення спроб введення ПІН-коду.");
                }
                await _cardRepo.UpdateAsync(card);
                await LogWarningAsync($"Невірний ПІН-код, Спроба {card.FailedAttempts} з 3", card.Id);
                throw new Exception("Невірний ПІН-код");
                
            }

            if (card.FailedAttempts > 0)
            {
                card.FailedAttempts = 0;
                await _cardRepo.UpdateAsync(card);
            }

            await LogInfoAsync("Спроба входу", card.Id);
            
            return true;
        }

        public async Task<bool> DepositCashAsync(Guid cardId, Dictionary<int, int> banknotes, string pin)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var card = await GetCardAsync(cardId);
                var account = await GetAccountAsync(card.AccountId);

                if (!_passwordHasher.VerifyPassword(pin, card.PinHash))
                {
                    await LogWarningAsync("невірний ПІН-код карти при ствробі зняття", cardId);
                    throw new Exception("невірний ПІН-код");
                }
                decimal totalDepositAmount = 0;
                foreach (var note in banknotes)
                {
                    if (note.Key <= 0 || note.Value <= 0)
                    {
                        await LogWarningAsync("Неправильні дані банкноти", cardId);
                        throw new Exception("Неправильні дані банкноти");
                    }

                    totalDepositAmount += (note.Key * note.Value);
                }
                if (totalDepositAmount == 0)
                {
                    await LogInfoAsync("Сума поповнення має бути більшою за нуль", cardId);
                    throw new Exception("Сума поповнення має бути більшою за нуль.");
                }

                var cassettes = await _cassetteRepo.GetAllAsync();

                foreach (var note in banknotes)
                {
                    var denomination = note.Key;
                    var countToAdd = note.Value;

                    var cassette = cassettes.FirstOrDefault(c => c.Denomination == denomination);

                    if (cassette == null)
                    {
                        await LogWarningAsync($"Банкомат не приймає банкноти такого номіналу {denomination}", cardId);
                        throw new Exception($"Банкомат не приймає банкноти такого номіналу {denomination}");
                    }

                    cassette.Count += countToAdd;
                    await _cassetteRepo.UpdateAsync(cassette);
                }

                account.Balance += totalDepositAmount;
                await _accountRepo.UpdateAsync(account);

                var newTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    Amount = totalDepositAmount,
                    TransactionType = "Поповнення",
                    TransactionDate = DateTime.UtcNow,
                    Status = "Успішно"
                };

                await _transactionRepo.AddAsync(newTransaction);
                await LogInfoAsync("Поповнення коштів", cardId);
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                await LogErrorAsync("Скасування операції депозиту", cardId);
                throw;
            }
        }

        public async Task<decimal> GetBalanceAsync(Guid cardId, string pin)
        {
            try
            {
                var card = await GetCardAsync(cardId);
                var account = await GetAccountAsync(card.AccountId);

                if (!_passwordHasher.VerifyPassword(pin, card.PinHash))
                {
                    await LogWarningAsync("невірний ПІН-код карти при спробі зняття", cardId);
                    throw new Exception("невірний ПІН-код");
                }

                await LogInfoAsync("Огляд рахунку", cardId);
                return account.Balance;
            }
            catch
            {
                await LogErrorAsync("Помилка огляду рахунку", cardId);
                throw new Exception("Помилка Операції");
            }
        }

        public async Task<bool> WithdrawCashAsync(Guid cardId, string pin, decimal amount)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var card = await GetCardAsync(cardId);
                if (card.IsBlocked)
                {
                    await LogWarningAsync("Картку заблоковано після 3 невдалих спроб", card.Id);
                    throw new Exception("Картку заблоковано через перевищення спроб введення ПІН-коду.");
                }
                var account = await GetAccountAsync(card.AccountId);

                if (!_passwordHasher.VerifyPassword(pin, card.PinHash))
                {
                    await LogWarningAsync("невірний ПІН-код карти при спробі зняття", cardId);
                    throw new Exception("невірний ПІН-код");
                }

                if (account.Balance < amount)
                {
                    await LogInfoAsync("Недостатньо коштів для зняття", cardId);
                    throw new Exception("Недостатньо коштів на рахунку");
                }

                var cassettes = await _cassetteRepo.GetAllAsync();

                var notesToDispense = _withdrawalStrategy.CalculateNotes(amount, cassettes);

                account.Balance -= amount;

                await _accountRepo.UpdateAsync(account);

                foreach (var item in notesToDispense)
                {
                    var cassetteToUpdate = item.Key;
                    cassetteToUpdate.Count -= item.Value;
                    await _cassetteRepo.UpdateAsync(cassetteToUpdate);
                }

                var newTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    Amount = -amount,
                    TransactionType = "Зняття",
                    TransactionDate = DateTime.UtcNow,
                    Status = "Успішно"
                };

                await _transactionRepo.AddAsync(newTransaction);

                await LogInfoAsync("Успішне зняття коштів", cardId);
                await transaction.CommitAsync();
                return true;
            }catch (Exception)
            {
                await LogErrorAsync("помилка операції зняття", cardId);
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<Card> GetCardAsync(Guid cardId)
        {
            
            var card = await _cardRepo.GetByCardByIdAsync(cardId);

            if (card == null) throw new Exception("Карту не знайдено");

            return card;
        }
        private async Task<Account> GetAccountAsync(Guid accountId)
        {
            var account = await _accountRepo.GetAccountByIdAsync(accountId);

            if (account == null) throw new Exception("Акаунт не знайдено");

            return account;
        }

        
    }
}
