using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;
using ATM.Domain.Interfaces;
using ATM.Domain.Interfaces.Services;
using ATM.Infrastructure.Data;
using ATM.Infrastructure.Repositories;

namespace ATM.Infrastructure.Services
{
    public class AtmService : IAtmService
    {
        private readonly AppDbContext _context;
        private ICardRepository _cardRepo;
        private IAccountRepository _accountRepo;
        private ITransactionRepository _transactionRepo;
        private IAtmCassetteRepository _cassetteRepo;
        private IAtmOperationLogRepository _operationLogRepo;
        private readonly IPasswordHasher _passwordHasher;

        public AtmService(AppDbContext context, ICardRepository cardRepo, IAccountRepository accountRepo,
            ITransactionRepository transactionRepo, IAtmCassetteRepository cassetteRepo,
            IAtmOperationLogRepository operationLogRepo, IPasswordHasher passwordHasher
            )
        {
            _context = context;
            _cardRepo = cardRepo;
            _accountRepo = accountRepo;
            _transactionRepo = transactionRepo;
            _cassetteRepo = cassetteRepo;
            _operationLogRepo = operationLogRepo;
            _passwordHasher = passwordHasher;
        }



        public async Task<bool> AuthenticateAsync(string cardNumber, string pin)
        {
            var card = await _cardRepo.GetByCardByNumberAsync(cardNumber);

            if (card == null)
            {
                await LogAsync("Невірний номер карти", "Warning");
                //var newLogIncorrectCard = new AtmOperationLog
                //{
                //    Id = Guid.NewGuid(),
                //    LogDate = DateTime.UtcNow,
                //    Message = "Невірний номер карти",
                //    LogLevel = "warning",
                //    StackTrace = null
                //};
                //await _operationLogRepo.AddAsync(newLogIncorrectCard);
                throw new Exception("Картку з таким номером не знайдено");

            }


            if (!_passwordHasher.VerifyPassword(pin, card.PinHash))
            {
                await LogAsync("Невірний ПІН-код", "Warning", card.Id);

                //var newLogIncorrectPINCode = new AtmOperationLog
                //{
                //    Id = Guid.NewGuid(),
                //    CardId = card.Id,
                //    LogDate = DateTime.UtcNow,
                //    Message = "Невірний ПІН-код",
                //    LogLevel = "warning",
                //    StackTrace = null
                //};
                //await _operationLogRepo.AddAsync(newLogIncorrectPINCode);
                throw new Exception("Невірний ПІН-код");
            }


            await LogAsync("Спроба входу", "Info", card.Id);
            //var newLogSuccessfulLogin = new AtmOperationLog
            //{
            //    Id = Guid.NewGuid(),
            //    CardId = card.Id,
            //    LogDate = DateTime.UtcNow,
            //    Message = "Спроба входу",
            //    LogLevel = "Info",
            //    StackTrace = null
            //};
            //await _operationLogRepo.AddAsync(newLogSuccessfulLogin);
            return true;
        }

        public async Task<bool> DepositCashAsync(Guid cardId, Dictionary<int, int> banknotes)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                decimal totalDepositAmount = 0;
                foreach (var note in banknotes)
                {
                    if (note.Key <= 0 || note.Value <= 0)
                    {
                        await LogAsync("Неправильні дані банкноти", "Warning", cardId);
                        throw new Exception("Неправильні дані банкноти");
                    }

                    totalDepositAmount += (note.Key * note.Value);
                }
                if (totalDepositAmount == 0)
                {
                    await LogAsync("Сума поповнення має бути більшою за нуль", "Info", cardId);
                    throw new Exception("Сума поповнення має бути більшою за нуль.");
                }

                var card = await GetCardAsync(cardId);
                var account = await GetAccountAsync(card.AccountId);

                var cassettes = await _cassetteRepo.GetAllAsync();

                foreach (var note in banknotes)
                {
                    var denomination = note.Key;
                    var countToAdd = note.Value;

                    var cassette = cassettes.FirstOrDefault(c => c.Denomination == denomination);

                    if (cassette == null)
                    {
                        await LogAsync($"Банкомат не приймає банкноти такого номіналу {denomination}", "Warning", cardId);
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
                };

                await _transactionRepo.AddAsync(newTransaction);
                await LogAsync("Поповнення коштів", "Info", cardId);
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                await LogAsync("Скасування операції", "Error", cardId);
                throw;
            }
        }

        public async Task<decimal> GetBalanceAsync(Guid cardId)
        {
            try
            {
                var card = await GetCardAsync(cardId);
                var account = await GetAccountAsync(card.AccountId);

                return account.Balance;
            }
            catch
            {
                throw new Exception("Помилка Операції");
            }
        }

        public async Task<bool> WithdrawCashAsync(Guid cardId, decimal amount)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var accountBalance = await GetBalanceAsync(cardId);

                if (accountBalance < amount) throw new Exception("Недостатньо коштів");

                var cassettes = await _cassetteRepo.GetAllAsync();
                var orderedCassettes = cassettes.OrderByDescending(c => c.Denomination).ToList();

                decimal remainingAmount = amount;

                var notesToDispense = new Dictionary<AtmCassette, int>();

                foreach (var cassette in orderedCassettes)
                {
                    if (remainingAmount == 0) break;

                    if (cassette.Count > 0 && cassette.Denomination <= remainingAmount)
                    {
                        int notesNeeded = (int)(remainingAmount / cassette.Denomination);

                        int notesToTake = Math.Min(notesNeeded, cassette.Count);

                        if (notesToTake > 0)
                        {
                            notesToDispense.Add(cassette, notesToTake);

                            remainingAmount -= (notesToTake * cassette.Denomination);
                        }
                    }
                }

                if (remainingAmount > 0)
                {
                    throw new Exception("Недостатньо коштів в банкоматі");
                }

                var card = await GetCardAsync(cardId);
                var account = await GetAccountAsync(card.AccountId);

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
                };

                await _transactionRepo.AddAsync(newTransaction);

                await transaction.CommitAsync();

                return true;
            }catch (Exception)
            {
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
        private async Task LogAsync( string message, string level, Guid? cartId = null)
        {
            await _operationLogRepo.AddAsync(new AtmOperationLog
            {
                Id = Guid.NewGuid(),
                CardId = cartId,
                LogDate = DateTime.UtcNow,
                Message = message,
                LogLevel = level
            });
        }
    }
}
