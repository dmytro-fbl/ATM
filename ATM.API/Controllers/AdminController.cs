using ATM.API.DTOs;
using ATM.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATM.API.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Admin/Dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet("api/admin/cassettes")]
        public async Task<IActionResult> GetCassettes()
        {
            try
            {
                var cassettes = await _context.AtmCassettes
                    .OrderBy(c => c.Denomination)
                    .ToListAsync();

                return Ok(cassettes);
            }
            catch (Exception ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("api/admin/refill")]
        public async Task<IActionResult> RefillCassette([FromBody] RefillRequest request)
        {
            var cassettes = await _context.AtmCassettes
                .FirstOrDefaultAsync(c => c.Denomination == request.Denomination);

            if (cassettes == null) return NotFound();

            cassettes.Count = request.NewCount;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Касета {request.Denomination} поповнена" ,
            currentCount = cassettes.Count});
        }
    }
}
