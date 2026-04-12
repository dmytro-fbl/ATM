using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;
using ATM.Domain.Interfaces;
using ATM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly AppDbContext _context;

        public CardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Card?> GetByCardByIdAsync(Guid cardId)
        {
            return await _context.Cards
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.Id == cardId);
        }

        public async Task<Card?> GetByCardByNumberAsync(string cardNumber)
        {
            return await _context.Cards
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
        }
        public async Task UpdateAsync(Card card)
        {
            _context.Cards.Update(card);
            await _context.SaveChangesAsync();
        } 
    }
}
