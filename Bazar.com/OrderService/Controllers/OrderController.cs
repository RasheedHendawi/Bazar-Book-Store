using Microsoft.AspNetCore.Mvc;
using OrderServer.Services;

namespace OrderServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost("purchase/{itemNumber}")]
        public async Task<IActionResult> Purchase(int itemNumber)
        {
            try
            {
                var result = await _orderService.PurchaseAsync(itemNumber);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Controller caught error: {ex.Message}");

                return BadRequest(ex.Message);
            }
        }
    }
}
