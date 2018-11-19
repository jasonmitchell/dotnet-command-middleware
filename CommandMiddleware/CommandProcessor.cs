using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public delegate Task CommandMiddleware(object command, Func<Task> next);
    public delegate Task CommandHandler<in TCommand>(TCommand command);
    public delegate Task ContextualCommandHandler<TCommand>(TCommand command, CommandContext<TCommand> context);
    public delegate Task CommandDelegate(object command);

    public class CommandProcessor : ICommandProcessorBuilder
    {
        private readonly List<CommandMiddleware> _middleware;
        private readonly Dictionary<Type, Func<object, Task>> _handlers;

        private CommandProcessor()
        {
            _middleware = new List<CommandMiddleware>();
            _handlers = new Dictionary<Type, Func<object, Task>>();
        }

        public static ICommandProcessorBuilder Use(CommandMiddleware middleware)
        {
            ICommandProcessorBuilder builder = new CommandProcessor();
            builder.Use(middleware);

            return builder;
        }

        public static ICommandProcessorBuilder Handle<TCommand>(CommandHandler<TCommand> handler)
        {
            ICommandProcessorBuilder builder = new CommandProcessor();
            builder.Handle(handler);

            return builder;
        }
        
        public static ICommandProcessorBuilder Handle<TCommand>(ContextualCommandHandler<TCommand> handler)
        {
            ICommandProcessorBuilder builder = new CommandProcessor();
            builder.Handle(handler);

            return builder;
        }

        ICommandProcessorBuilder ICommandProcessorBuilder.Use(CommandMiddleware middleware)
        {
            _middleware.Add(middleware);
            return this;
        }

        ICommandProcessorBuilder ICommandProcessorBuilder.Handle<TCommand>(CommandHandler<TCommand> handler)
        {
            _handlers.Add(typeof(TCommand), x => handler((TCommand) x));
            return this;
        }
        
        ICommandProcessorBuilder ICommandProcessorBuilder.Handle<TCommand>(ContextualCommandHandler<TCommand> handler)
        {
            _handlers.Add(typeof(TCommand), x => handler((TCommand) x, new CommandContext<TCommand>((TCommand)x)));
            return this;
        }
        
        CommandDelegate ICommandProcessorBuilder.Build()
        {
            _middleware.Insert(0, RequireHandler);
            
            var pipeline = _middleware.Any() ? CreatePipeline(0) : Execute;
            return c => pipeline(c);
        }

        private async Task RequireHandler(object command, Func<Task> next)
        {
            if (!_handlers.ContainsKey(command.GetType()))
            {
                throw new InvalidOperationException($"Handler for {command.GetType().Name} not found");
            }

            await next();
        }

        private Func<object, Task> CreatePipeline(int index)
        {
            return async c =>
            {
                Func<Task> next;

                if (index >= _middleware.Count - 1)
                {
                    next = () => Execute(c);
                }
                else
                {
                    var nextIndex = index + 1;
                    var nextMiddleware = CreatePipeline(nextIndex);
                    next = () => nextMiddleware(c);
                }
                
                var middleware = _middleware[index];
                await middleware(c, next);
            };
        }
        
        private async Task Execute(object c)
        {
            var handler = _handlers[c.GetType()];
            await handler(c);
        }
    }
}