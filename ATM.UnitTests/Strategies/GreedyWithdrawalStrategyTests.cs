using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;
using ATM.Infrastructure.Strategies;

namespace ATM.UnitTests.Strategies
{
    public class GreedyWithdrawalStrategyTests
    {
        [Fact]
        public void CalculateNotes_ShouldCorrectReturnCorrectCount_WhenAmountIsPossible()
        {
            var strategy = new GreedyWithdrawalStrategy();
            var cassettes = new List<AtmCassette>
            {
                 new AtmCassette { Denomination = 500, Count = 10},
                 new AtmCassette { Denomination = 200, Count = 10},
                 new AtmCassette { Denomination = 100, Count = 10}
            };

            decimal amount = 800;

            var result = strategy.CalculateNotes(amount, cassettes);

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[cassettes.Find(c => c.Denomination == 500)]);
            Assert.Equal(1, result[cassettes.Find(c => c.Denomination == 200)]);
            Assert.Equal(1, result[cassettes.Find(c => c.Denomination == 100)]);
        }
    }
}
