using ATM.API.DTOs;
using ATM.Domain.Interfaces;
using ATM.Domain.Interfaces.Services;
using ATM.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace ATM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AtmController : ControllerBase
    {
        private readonly IAtmService _atmService;
        private readonly ICardRepository _cardRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly InputValidator _inputValidator;
        private readonly IReceiptService _receiptService;

        public AtmController(IAtmService atmService, ICardRepository cardRepo, ITransactionRepository transactionRepo, InputValidator inputValidator, IReceiptService receiptService)
        {
            _atmService = atmService;
            _cardRepo = cardRepo;
            _transactionRepo = transactionRepo;
            _inputValidator = inputValidator;
            _receiptService = receiptService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var card = await _cardRepo.GetByCardByNumberAsync(request.CardNumber);

            if (card == null) return Unauthorized(new { message = "Карту не знайдено" });
            bool isSuccess = await _atmService.AuthenticateAsync(request.CardNumber, request.Pin);

            if (isSuccess)
                return Ok(new {message = "Успішний вхід", isAuthentificated = true,
                    cardId = card.Id, isAdmin = card.IsAdmin});
            
            return Unauthorized(new { message = "невірний номер картки або ПІН-код" });
            
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
        {
            try
            {
                if (!_inputValidator.IsValidWithdrawalAmount(request.Amount))
                    return BadRequest(new { message = "Перевищено ліміт зняття за одну операцію (макс. 20 000 USD)" });
                bool isSuccess = await _atmService.WithdrawCashAsync(request.CardId, request.Pin, request.Amount);

                var balance = await _atmService.GetBalanceAsync(request.CardId, request.Pin);

                var card = await _cardRepo.GetByCardByIdAsync(request.CardId);

                string receiptText = _receiptService.GenerateAtmReceipt(
                    cardNumber: card.CardNumber,
                    transactionType: "Withdrawal",
                    amount: request.Amount,
                    balance: balance);


                return Ok(new { message = "Гроші видано", receipt = receiptText });

            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message });
            }
        }

        [HttpPost("balance")]
        public async Task<IActionResult> Balance([FromBody] GetBalanceRequest request)
        {
            var balance = await _atmService.GetBalanceAsync(request.CardId, request.Pin);
            return Ok(new { balance = balance, currency = "UAH"});
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            try
            {
                bool isSuccess = await _atmService.DepositCashAsync(request.CardId, request.Banknotes, request.Pin);

                var balance = await _atmService.GetBalanceAsync(request.CardId, request.Pin);

                var card = await _cardRepo.GetByCardByIdAsync(request.CardId);

                string receiptText = _receiptService.GenerateAtmReceipt(
                    cardNumber: card.CardNumber,
                    transactionType: "Deposit",
                    amount: request.Amount,
                    balance: balance);

                return Ok(new { message = "Успішне поповнення", receipt = receiptText});
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("history/{cardId}")]
        public async Task<IActionResult> GetHistory(Guid cardId, [FromQuery] int page = 1)
        {
            try
            {
                int pageSize = 10;
                var (item, totalCount) = await _atmService.GetTransactionsAsync(cardId, page, pageSize);

                var result = item.Select(t => new
                {
                    date = t.TransactionDate.ToString("dd.MM.yyyy HH:mm"),
                    type = t.TransactionType.ToString(),
                    amount = t.Amount
                });

                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                return Ok(new
                {
                    transactions = result,
                    totalPages = totalPages,
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
               return BadRequest(new {message =  ex.Message});
            }
        }



    }
}
