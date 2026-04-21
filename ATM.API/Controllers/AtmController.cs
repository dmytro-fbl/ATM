using ATM.API.DTOs;
using ATM.Domain.Interfaces;
using ATM.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AtmController : ControllerBase
    {
        private readonly IAtmService _atmService;
        private readonly ICardRepository _cardRepo;

        public AtmController(IAtmService atmService, ICardRepository cardRepo)
        {
            _atmService = atmService;
            _cardRepo = cardRepo;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var card = await _cardRepo.GetByCardByNumberAsync(request.CardNumber);

            if (card == null) return Unauthorized(new { message = "Карту не знайдено" });
            bool isSuccess = await _atmService.AuthenticateAsync(request.CardNumber, request.Pin);

            if (isSuccess)
                return Ok(new {message = "Успішний вхід", isAuthentificated = true, cardId = card.Id});
            
            return Unauthorized(new { message = "невірний номер картки або ПІН-код" });
            
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
        {
            try
            {
                bool isSuccess = await _atmService.WithdrawCashAsync(request.CardId, request.Pin, request.Amount);
                return Ok(new { message = "Гроші видано" });

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
                return Ok(new { message = "Успішне поповнення" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
