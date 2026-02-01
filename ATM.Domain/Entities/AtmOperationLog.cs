using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Domain.Entities
{
    public class AtmOperationLog
    {
        public Guid Id { get; set; }
        public DateTime LogDate { get; set; } = DateTime.Now;
        public string LogLevel { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? StackTrace {  get; set; } = string.Empty;
    }
}
