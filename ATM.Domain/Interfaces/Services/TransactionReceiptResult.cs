using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Domain.Interfaces.Services
{
    public class TransactionReceiptResult
    {
        public string Message { get; set; } = string.Empty;
        public string Receipt { get; set; } = string.Empty;
    }
}
