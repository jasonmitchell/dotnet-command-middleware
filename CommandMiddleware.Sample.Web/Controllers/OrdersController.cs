using System.Threading.Tasks;
using CommandMiddleware.Sample.Web.Commands;
using Microsoft.AspNetCore.Mvc;

namespace CommandMiddleware.Sample.Web.Controllers
{
    [ApiController, Route("[controller]")]
    public class OrdersController : Controller
    {
        private readonly CommandDelegate _commandProcessor;

        public OrdersController(CommandDelegate commandProcessor)
        {
            _commandProcessor = commandProcessor;
        }

        [HttpPost, Route("checkout")]
        public async Task<ActionResult> Checkout(Checkout command)
        {
            await _commandProcessor(command);
            
            return new CreatedResult("hello/world", null);
        }
    }
}