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
    public class AtmOperationLogRepository : IAtmOperationLogRepository
    {
        private readonly AppDbContext _context;

        public AtmOperationLogRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(AtmOperationLog log)
        {
            _context.AtmOperationLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AtmOperationLog>> GetAllAsync()
        {
            return await _context.AtmOperationLogs
                .OrderByDescending(l => l.LogDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AtmOperationLog>> GetByCardIdAsync(Guid cardId)
        {
            return await _context.AtmOperationLogs
                .Where(l => l.CardId == cardId)
                .ToListAsync();
        }
    }
}
