using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public virtual Account Account { get; set; } = null!;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = null!;
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = null!;

    }
}
