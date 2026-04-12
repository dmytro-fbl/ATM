using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;

namespace ATM.Domain.Interfaces
{
    public interface ICardRepository
    {
        public Task<Card?> GetByCardByNumberAsync(string cardNumber);
        public Task<Card?> GetByCardByIdAsync(Guid cardId);
        public Task UpdateAsync(Card card);
    }
}
