using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Domain.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public decimal Balance { get; set; } = 0.00m;
        public string Currency { get; set; } = "UAH";

        public virtual ICollection<Card> Cards { get; set; } = new List<Card>();

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        
    }
}
