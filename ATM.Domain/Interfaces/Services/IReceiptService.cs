using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Domain.Interfaces.Services
{
    public interface IReceiptService
    {
        string GenerateAtmReceipt(string cardNumber, string transactionType,
            decimal amount, decimal balance, string currency = "USD");
    }
}
