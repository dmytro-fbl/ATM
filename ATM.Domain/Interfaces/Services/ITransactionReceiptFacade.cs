using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Domain.Interfaces.Services
{
    public interface ITransactionReceiptFacade
    {
        Task<TransactionReceiptResult> HandleWithdrawAsync(Guid cardId, string pin, decimal amount);
        Task<TransactionReceiptResult> HandleDepositAsync(Guid cardId, string pin, Dictionary<int, int> banknotes, decimal amount);
    }
}
