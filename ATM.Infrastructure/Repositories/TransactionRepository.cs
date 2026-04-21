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
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;
        public TransactionRepository(AppDbContext context)
        {
            _context = context; 
        } 
        public async Task AddAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .ToListAsync();
        }

        public Task<Transaction?> GetByIdAsync(Guid id)
        {
            return _context.Transactions
               .FirstOrDefaultAsync(t => t.Id == id);
            
        }

        public async Task<IEnumerable<Transaction>> GetRecentTransactionAsync(Guid accountId, int count)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .Take(count)
                .ToListAsync(); 
        }

        public async Task<(IEnumerable<Transaction> Items, int TotalCount)> GetPaginatedByAccountIdAsync(Guid accountId, int page, int pageSize)
        {
            var query = _context.Transactions.Where(t => t.AccountId == accountId);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
