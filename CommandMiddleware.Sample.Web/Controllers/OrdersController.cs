using System.Threading.Tasks;
using CommandMiddleware.Sample.Web.Commands;
using Microsoft.AspNetCore.Mvc;

namespace CommandMiddleware.Sample.Web.Controllers
{
    [ApiController, Route("[controller]")]
    public class OrdersController : Controller
    {
        private readonly HttpCommandDelegate _commandProcessor;

        public OrdersController(HttpCommandDelegate commandProcessor)
        {
            _commandProcessor = commandProcessor;
        }
        
        [HttpPost, Route("add-item")]
        public async Task<ActionResult> AddItem(AddItemToBasket command)
        {
            return await _commandProcessor(command, HttpContext);
        }

        [HttpPost, Route("checkout")]
        public async Task<ActionResult> Checkout(Checkout command)
        {
            return await _commandProcessor(command, HttpContext);
        }
    }
}