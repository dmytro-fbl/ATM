using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;
using ATM.Domain.Interfaces.Strategies;

namespace ATM.Infrastructure.Strategies
{
    public class GreedyWithdrawalStrategy : ICashWithdrawalStrategy
    {
        public Dictionary<AtmCassette, int> CalculateNotes(decimal amount, IEnumerable<AtmCassette> cassettes)
        {
            var orderedCassettes = cassettes.OrderByDescending(c => c.Denomination).ToList();

            decimal remainingAmount = amount;

            var notesToDispense = new Dictionary<AtmCassette, int>();

            foreach (var cassette in orderedCassettes)
            {
                if (remainingAmount == 0) break;

                if (cassette.Count > 0 && cassette.Denomination <= remainingAmount)
                {
                    int notesNeeded = (int)(remainingAmount / cassette.Denomination);

                    int notesToTake = Math.Min(notesNeeded, cassette.Count);

                    if (notesToTake > 0)
                    {
                        notesToDispense.Add(cassette, notesToTake);

                        remainingAmount -= (notesToTake * cassette.Denomination);
                    }
                }
            }

            if (remainingAmount > 0)
            {
                throw new Exception("Недостатньо коштів в банкоматі");
            }
            return notesToDispense;
        }
    }
}
