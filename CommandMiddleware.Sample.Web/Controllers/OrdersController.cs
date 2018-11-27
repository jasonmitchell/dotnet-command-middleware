using System.Threading.Tasks;
using CommandMiddleware.Sample.Web.Commands;
using CommandMiddleware.Sample.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using OneOf;

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
        
        [HttpPost, Route("add-item")]
        public async Task<ActionResult> AddItem(AddItemToBasket command)
        {
            await _commandProcessor(command);
            return Ok();
            //return await _commandProcessor(command);
        }

        [HttpPost, Route("checkout")]
        public async Task<ActionResult> Checkout(Checkout command)
        {
            //return await _commandProcessor(command);
            
            return (await _commandProcessor(command)).ResultAs<OneOf<string, DomainError>>().Match<ActionResult>(
                s => Created($"/somewhere/{s}", null),
                error =>
                {
                    ModelState.AddModelError(error.Key, error.Message);
                    return BadRequest(ModelState);
                }
            );
        }
    }
}