using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public delegate Task CommandMiddleware(object command, Func<Task> next);
    public delegate Task ContextualCommandMiddleware(object command, CommandContext context, Func<Task> next);
    
    public delegate Task CommandHandler<in TCommand>(TCommand command);
    public delegate Task ContextualCommandHandler<in TCommand>(TCommand command, CommandContext context);
    
    public delegate Task<CommandContext> CommandDelegate(object command);

    public class CommandProcessor : ICommandProcessorBuilder
    {
        private readonly List<ContextualCommandMiddleware> _middleware;
        private readonly Dictionary<Type, Func<object, CommandContext, Task>> _handlers;

        private CommandProcessor()
        {
            _middleware = new List<ContextualCommandMiddleware>();
            _handlers = new Dictionary<Type, Func<object, CommandContext, Task>>();
            
            _middleware.Add(RequireHandler);
        }

        public static ICommandProcessorBuilder Use(CommandMiddleware middleware)
        {
            ICommandProcessorBuilder builder = new CommandProcessor();
            builder.Use(middleware);

            return builder;
        }
        
        public static ICommandProcessorBuilder Use(ContextualCommandMiddleware middleware)
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
            _middleware.Add((c, _, next) => middleware(c, next));
            return this;
        }
        
        ICommandProcessorBuilder ICommandProcessorBuilder.Use(ContextualCommandMiddleware middleware)
        {
            _middleware.Add(middleware);
            return this;
        }

        ICommandProcessorBuilder ICommandProcessorBuilder.Handle<TCommand>(CommandHandler<TCommand> handler)
        {
            _handlers.Add(typeof(TCommand), (c, _) => handler((TCommand) c));
            return this;
        }
        
        ICommandProcessorBuilder ICommandProcessorBuilder.Handle<TCommand>(ContextualCommandHandler<TCommand> handler)
        {
            _handlers.Add(typeof(TCommand), (c, ctx) => handler((TCommand) c, ctx));
            return this;
        }
        
        CommandDelegate ICommandProcessorBuilder.Build()
        {
            _middleware.Add(ExecuteCommand);
            
            var pipeline = CreatePipeline(0);
            return c => pipeline(c, new CommandContext());
        }

        private Func<object, CommandContext, Task<CommandContext>> CreatePipeline(int index)
        {
            return async (command, context) =>
            {
                Func<Task> next;

                if (index >= _middleware.Count - 1)
                {
                    next = () => Task.CompletedTask;
                }
                else
                {
                    var nextIndex = index + 1;
                    var nextMiddleware = CreatePipeline(nextIndex);
                    next = () => nextMiddleware(command, context);
                }
                
                var middleware = _middleware[index];
                await middleware(command, context, next);

                return context;
            };
        }
        
        private async Task RequireHandler(object command, CommandContext context, Func<Task> next)
        {
            if (!_handlers.ContainsKey(command.GetType()))
            {
                throw new InvalidOperationException($"Handler for {command.GetType().Name} not found");
            }

            await next();
        }

        private async Task ExecuteCommand(object command, CommandContext context, Func<Task> _)
        {
            var handler = _handlers[command.GetType()];
            await handler(command, context);
            
            context.Complete();
        }
    }
}