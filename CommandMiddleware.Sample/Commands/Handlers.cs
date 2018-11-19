using System;
using System.Threading.Tasks;

namespace CommandMiddleware.Sample.Commands
{
    public static class Handlers
    {
        public static Task Handle(AddItemToBasket command)
        {
            Console.WriteLine($"\tAdding {command.ItemId} to basket");
            return Task.CompletedTask;
        }

        public static Task Handle(Checkout command, CommandContext<Checkout> context)
        {
            Console.WriteLine($"\tPlacing order for {command.Items.Count} items");

            foreach (var item in command.Items)
            {
                Console.WriteLine($"\t\t{item}");
            }

            return Task.CompletedTask;
        }
    }
}