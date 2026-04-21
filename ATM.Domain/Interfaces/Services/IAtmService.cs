using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;

namespace ATM.Domain.Interfaces.Services
{
    public interface IAtmService
    {
        Task<bool> AuthenticateAsync(string cardNumber, string pin);

        Task<decimal> GetBalanceAsync(Guid cardId, string pin);

        Task<bool> WithdrawCashAsync(Guid cardId, string pin, decimal amount);

        Task<bool> DepositCashAsync(Guid cardId, Dictionary<int, int> banknotes, string pin);

        Task<IEnumerable<Transaction>> GetTransactionsAsync(Guid cardId);
    }
}
