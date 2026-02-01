using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }


        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? phoneNumber { get; set; } = string.Empty;

        public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}
