using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;

namespace ATM.Domain.Interfaces
{
    public interface IAtmOperationLogRepository
    {
        Task AddAsync(AtmOperationLog log);
        Task<IEnumerable<AtmOperationLog>> GetAllAsync();
        Task<IEnumerable<AtmOperationLog>> GetByCardIdAsync(Guid cardId);
    }
}
