using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;

namespace ATM.Domain.Interfaces.Base
{
    public abstract class BaseService
    {
        private readonly IAtmOperationLogRepository _operationLogRepo;

        protected BaseService(IAtmOperationLogRepository operationLogRepo)
        {
            _operationLogRepo = operationLogRepo;
        }

        protected async Task LogInfoAsync(string message, Guid? cardId = null)
        {
            await WriteLogAsync(message, "Info", cardId);
        }
        protected async Task LogWarningAsync(string message, Guid? cardId = null)
        {
            await WriteLogAsync(message, "Warning", cardId);
        }

        protected async Task LogErrorAsync(string message, Guid? cardId = null)
        {
            await WriteLogAsync(message, "Error", cardId);
        }

        private async Task WriteLogAsync(string message, string level, Guid? cardId)
        {
            var log = new AtmOperationLog
            {
                Id = Guid.NewGuid(),
                LogDate = DateTime.UtcNow,
                LogLevel = level,
                Message = message,
                CardId = cardId
            };
            await _operationLogRepo.AddAsync(log);
        }

        
    }
}
