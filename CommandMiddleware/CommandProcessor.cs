using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public delegate Task<CommandResult> CommandDelegate(object command);

    public class CommandProcessor
    {
        public delegate Task Middleware(object command, Func<Task> next);
        public delegate Task MiddlewareWithContext(object command, CommandContext context, Func<Task> next);
        public delegate Task<CommandResult> Handler<in TCommand>(TCommand command);
        
        private readonly List<MiddlewareWithContext> _middleware = new List<MiddlewareWithContext>();
        private readonly Dictionary<Type, Func<object, Task<CommandResult>>> _handlers = new Dictionary<Type, Func<object, Task<CommandResult>>>();

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
            if (_handlers.ContainsKey(typeof(TCommand)))
            {
                throw new CommandHandlerAlreadyRegisteredException(typeof(TCommand));
            }
            
            _handlers.Add(typeof(TCommand), c => handler((TCommand)c));
            return this;
        }

        public CommandDelegate Build()
        {
            _middleware.Add(ExecuteCommand);

            var pipeline = CreatePipeline(0);
            return c => pipeline(c, new CommandContext());
        }

        private Func<object, CommandContext, Task<CommandResult>> CreatePipeline(int index)
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

                return context.Result;
            };
        }

        private async Task RequireHandler(object command, CommandContext context, Func<Task> next)
        {
            var commandType = command.GetType();
            if (!_handlers.ContainsKey(commandType))
            {
                throw new CommandHandlerNotFoundException(commandType);
            }

            await next();
        }

        private async Task ExecuteCommand(object command, CommandContext context, Func<Task> _)
        {
            var handler = _handlers[command.GetType()];
            var commandResult = await handler(command);
            
            context.Result = commandResult;
        }
    }
}