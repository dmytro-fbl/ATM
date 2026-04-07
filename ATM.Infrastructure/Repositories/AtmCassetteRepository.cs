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
    public class AtmCassetteRepository : IAtmCassetteRepository
    {
        private readonly AppDbContext _context;

        public AtmCassetteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AtmCassette>> GetAllAsync()
        {
            return await _context.AtmCassettes
                .ToListAsync();
        }

        public async Task<AtmCassette?> GetByDenominationAsync(int denomination)
        {
            return await _context.AtmCassettes
                .FirstOrDefaultAsync(a => a.Denomination == denomination);
        }

        public async Task UpdateAsync(AtmCassette cassette)
        {
            _context.AtmCassettes.Update(cassette);
            await _context.SaveChangesAsync();
        }
    }
}
