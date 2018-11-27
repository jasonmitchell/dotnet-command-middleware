using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CommandMiddleware.Sample.Web.Commands
{
    public class Handlers
    {
        private readonly ILogger _logger;

        public Handlers(ILogger logger)
        {
            _logger = logger;
        }
        
        public Task Handle(AddItemToBasket command)
        {
            _logger.LogInformation($"Adding {command.ItemId} to basket");
            return Task.CompletedTask;
        }

        public async Task<string> Handle(Checkout command)
        {
            _logger.LogInformation($"Placing order for {command.Items.Count} items");

            await Task.CompletedTask;
            return "Hello world";
        }
    }
}