using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;

namespace ATM.Domain.Interfaces
{
    public interface IAtmCassetteRepository
    {
        Task<IEnumerable<AtmCassette>> GetAllAsync();
        Task<AtmCassette?> GetByDenominationAsync(int denomitation);
        Task UpdateAsync(AtmCassette cassette);
    }
}
