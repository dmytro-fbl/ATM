using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;

namespace ATM.Domain.Interfaces.Strategies
{
    public interface ICashWithdrawalStrategy
    {
        Dictionary<AtmCassette, int> CalculateNotes(decimal amount, IEnumerable<AtmCassette> cassettes);
    }
}
