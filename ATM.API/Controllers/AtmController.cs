using ATM.API.DTOs;
using ATM.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AtmController : ControllerBase
    {
        private readonly IAtmService _atmService;

        public AtmController(IAtmService atmService)
        {
            _atmService = atmService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            bool isSuccess = await _atmService.AuthenticateAsync(request.CardNumber, request.Pin);

            if (isSuccess)
            {
                return Ok(new {message = "Успішний вхід", isAuthentificated = true});
            }
            else
            {
                return Unauthorized(new { message = "невірний номер картки або ПІН-код" });
            }
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
        {
            bool isSuccess = await _atmService.WithdrawCashAsync(request.CardId, request.PinHash, request.Amount);

            if (isSuccess)
            {
                return Ok(new { message = "Гроші видано" });
            }
            else
            {
                throw new Exception("Помилка операції");
            }
        }

        [HttpGet("balance/{cardId}")]
        public async Task<IActionResult> Balance([FromRoute] Guid cardId)
        {
            var balance = await _atmService.GetBalanceAsync(cardId);
            return Ok(new { balance = balance, currency = "UAH"});
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            bool isSuccess = await _atmService.DepositCashAsync(request.CardId, request.Banknotes);

            if (isSuccess)
            {
                return Ok(new { message = "Успішне поповнення" });
            }
            else
            {
                throw new Exception("Помилка операції");
            }
        }
    }
}
