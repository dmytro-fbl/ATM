using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Interfaces;
using ATM.Domain.Entities;
using ATM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace ATM.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetAccountByIdAsync(Guid accountId)
        {
            return await _context.Accounts
                .Include(a => a.Card)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == accountId);
        }
        public async Task<IEnumerable<Account>> GetAccountsByCardIdAsync(Guid cardId)
        {
            return await _context.Accounts
                .Where(a => a.CardId == cardId)
                .ToListAsync();
        }
        public async Task UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }
    }
}
