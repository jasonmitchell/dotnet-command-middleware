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
        
        public async Task<CommandResult> Handle(AddItemToBasket command)
        {
            await Task.Run(() => _logger.LogInformation($"Adding {command.ItemId} to basket"));
            return Command.Handled();
        }

        public async Task<CommandResult> Handle(Checkout command)
        {
            await Task.Run(() => _logger.LogInformation($"Placing order for {command.Items.Count} items"));
            return Command.Handled(Guid.NewGuid());
        }
    }
}