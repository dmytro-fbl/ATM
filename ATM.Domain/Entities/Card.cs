using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Domain.Entities
{
    public class Card
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CardNumber { get; set; }
        public string PinHash { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsBlocked { get; set; } = false;
        public int FailedAttempts { get; set; } = 0;

        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
