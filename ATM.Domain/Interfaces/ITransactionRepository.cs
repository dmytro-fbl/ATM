using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;


namespace ATM.Domain.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);

        Task<Transaction?> GetByIdAsync(Guid id);

        Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId);

        Task<IEnumerable<Transaction>> GetRecentTransactionAsync(Guid accountId, int count);
    }
}
