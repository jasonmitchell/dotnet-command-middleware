using System;
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

        [HttpPost, Route("add-item")]
        public async Task<ActionResult> AddItem(AddItemToBasket command)
        {
            return await _commandProcessor(command).ToActionResult();
        }

        [HttpPost, Route("checkout")]
        public async Task<ActionResult> Checkout(Checkout command)
        {
            return await _commandProcessor(command)
                .Match<Guid>(id => Created($"/orders/{id}", null))
                .ToActionResult();
        }
    }
}