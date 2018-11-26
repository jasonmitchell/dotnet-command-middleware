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

    public class CommandProcessorBuilder
    {
        private readonly List<ContextualCommandMiddleware> _middleware = new List<ContextualCommandMiddleware>();
        private readonly Dictionary<Type, Func<object, CommandContext, Task>> _handlers = new Dictionary<Type, Func<object, CommandContext, Task>>();

        public CommandProcessorBuilder()
        {
            _middleware.Add(RequireHandler);
        }

        public CommandProcessorBuilder Use(CommandMiddleware middleware) => 
            Use((c, _, next) => middleware(c, next));

        public CommandProcessorBuilder Use(ContextualCommandMiddleware middleware)
        {
            _middleware.Add(middleware);
            return this;
        }

        public CommandProcessorBuilder Handle<TCommand>(CommandHandler<TCommand> handler) =>
            Handle<TCommand>((c, _) => handler(c));

        public CommandProcessorBuilder Handle<TCommand>(ContextualCommandHandler<TCommand> handler)
        {
            if (_handlers.ContainsKey(typeof(TCommand)))
            {
                throw new CommandHandlerAlreadyRegisteredException(typeof(TCommand));
            }
            
            _handlers.Add(typeof(TCommand), (c, ctx) => handler((TCommand) c, ctx));
            return this;
        }

        public CommandDelegate Build()
        {
            _middleware.Add(ExecuteCommand);

            var pipeline = CreatePipeline(0);
            return c => pipeline(c, new CommandContext(c.GetType()));
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
                throw new CommandHandlerNotFoundException(context.CommandType);
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