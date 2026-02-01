using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Domain.Entities
{
    public class AtmCassette
    {
        public int Id { get; set; }
        public int Denomination { get; set; }
        public int Count { get; set; } = 0;
    }
}
