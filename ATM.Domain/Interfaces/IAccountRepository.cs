using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;

namespace ATM.Domain.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetAccountByIdAsync(Guid accountId);
        Task<IEnumerable<Account>> GetAccountsByCardIdAsync(Guid cardId);
        Task UpdateAsync(Account account);
    }
}
