using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandMiddleware.Sample.Commands;

namespace CommandMiddleware.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processor = CreateCommandProcessor();
            var itemIds = new List<Guid>();

            for (var i = 0; i < 5; i++)
            {
                var id = Guid.NewGuid();
                itemIds.Add(id);

                await processor(new AddItemToBasket {ItemId = id});
            }

            await processor(new Checkout {Items = itemIds});

            Console.ReadLine();
        }

        private static CommandDelegate CreateCommandProcessor() =>
            CommandProcessor
                .Use(LogCommand)
                .Handle<AddItemToBasket>(Handlers.Handle)
                .Handle<Checkout>(Handlers.Handle)
                .Build();

        private static async Task LogCommand(object command, Func<Task> next)
        {
            Console.WriteLine($"-- Processing {command.GetType().Name} --");
            await next();
        }
    }
}