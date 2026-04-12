using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;
using ATM.Domain.Interfaces;
using ATM.Domain.Interfaces.Services;
using ATM.Infrastructure.Repositories;

namespace ATM.Infrastructure.Services
{
    public class AtmService : IAtmService
    {
        private CardRepository _cardRepo;
        private AccountRepository _accountRepo;
        private TransactionRepository _transactionRepo;
        private AtmCassetteRepository _cassetteRepo;
        private AtmOperationLogRepository _operationLogRepo;
        private readonly IPasswordHasher _passwordHasher;

        public async Task<bool> AuthenticateAsync(string cardNumber, string pin)
        {
            var card = await _cardRepo.GetByCardByNumberAsync(cardNumber);

            if (card == null) return false;

            return _passwordHasher.VerifyPassword(pin, card.PinHash);
        }

        public async Task DepositCashAsync(Guid cardId, Dictionary<int, int> banknotes)
        {
            decimal totalDepositAmount = 0;
            foreach (var note in banknotes)
            {
                if (note.Key <= 0 || note.Value <= 0)
                    throw new Exception("Incorrect banknote data");

                totalDepositAmount += (note.Key * note.Value);
            }
            if (totalDepositAmount == 0)
                throw new Exception("The top-up amount must be greater than zero.");

            var card = await GetCard(cardId);
            var account = await GetAccount(card.AccountId);

            var cassettes = await _cassetteRepo.GetAllAsync();

            foreach (var note in banknotes)
            {
                var denomination = note.Key;
                var countToAdd = note.Value;

                var cassette = cassettes.FirstOrDefault(c => c.Denomination == denomination);

                if (cassette == null)
                {
                    throw new Exception($"The ATM does not accept banknotes of the same denomination {denomination}");
                }

                cassette.Count += countToAdd;
                await _cassetteRepo.UpdateAsync(cassette);
            }
        }

        public async Task<decimal> GetBalanceAsync(Guid cardId)
        {
            var card = await GetCard(cardId);
            var account = await GetAccount(card.AccountId);            

            return account.Balance;
        }

        public async Task<bool> WithdrawCashAsync(Guid cardId, decimal amount)
        {
            var accountBalance = await GetBalanceAsync(cardId);

            if (accountBalance < amount) throw new Exception("not enough money");

            var cassettes = await _cassetteRepo.GetAllAsync();
            var orderedCassettes = cassettes.OrderByDescending(c => c.Denomination).ToList();

            decimal remainingAmount = amount;

            var notesToDispense = new Dictionary<AtmCassette, int>();

            foreach (var cassette in orderedCassettes)
            {
                if (remainingAmount == 0) break;

                if(cassette.Count > 0 && cassette.Denomination <= remainingAmount)
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

            if( remainingAmount > 0)
            {
                throw new Exception("not enough money in the ATM");
            }

            var card = await GetCard(cardId);
            var account = await GetAccount(card.AccountId);

            account.Balance -= amount;

            await _accountRepo.UpdateAsync(account);

            foreach (var item in notesToDispense)
            {
                var cassetteToUpdate = item.Key;
                cassetteToUpdate.Count -= item.Value;
                await _cassetteRepo.UpdateAsync(cassetteToUpdate);
            }

            return true;
        }

        private async Task<Card> GetCard(Guid cardId)
        {
            var card = await _cardRepo.GetByCardByIdAsync(cardId);

            if (card == null) throw new Exception("Card not found");

            return card;
        }
        private async Task<Account> GetAccount(Guid accountId)
        {
            var account = await _accountRepo.GetAccountByIdAsync(accountId);

            if (account == null) throw new Exception("Account not found");

            return account;
        }
    }
}
