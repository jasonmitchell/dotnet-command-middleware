using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public delegate Task<CommandContext> CommandDelegate(object command);

    public class CommandProcessor
    {
        public delegate Task Middleware(object command, Func<Task> next);
        public delegate Task MiddlewareWithContext(object command, CommandContext context, Func<Task> next);
        public delegate Task Handler<in TCommand>(TCommand command);
        public delegate Task HandlerWithContext<in TCommand>(TCommand command, CommandContext context);
        
        private readonly List<MiddlewareWithContext> _middleware = new List<MiddlewareWithContext>();
        private readonly Dictionary<Type, Func<object, CommandContext, Task>> _handlers = new Dictionary<Type, Func<object, CommandContext, Task>>();

        public CommandProcessor()
        {
            _middleware.Add(RequireHandler);
        }

        public CommandProcessor Use(Middleware middleware)
        {
            return Use((c, _, next) => middleware(c, next));
        }

        public CommandProcessor Use(MiddlewareWithContext middleware)
        {
            _middleware.Add(middleware);
            return this;
        }

        public CommandProcessor Handle<TCommand>(Handler<TCommand> handler)
        {
            return Handle<TCommand>((c, _) => handler(c));
        }

        public CommandProcessor Handle<TCommand>(HandlerWithContext<TCommand> handler)
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