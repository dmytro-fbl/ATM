using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Domain.Interfaces.Services
{
    public interface IAtmService
    {
        Task<bool> AuthenticateAsync(string cardNumber, string pin);

        Task<decimal> GetBalanceAsync(Guid cardId);

        Task<bool> WithdrawCashAsync(Guid cardId, decimal amount);

        Task DepositCashAsync(Guid cardId, Dictionary<int, int> banknotes);
    }
}
