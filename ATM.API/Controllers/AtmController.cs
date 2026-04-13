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
    }
}
