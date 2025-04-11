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
            var result = await _orderService.PurchaseAsync(itemNumber);
            return Ok(new { message = result });
        }
    }
}
