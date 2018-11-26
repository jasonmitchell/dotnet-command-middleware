using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public delegate Task CommandMiddleware(object command, Func<Task> next);
    public delegate Task ContextualCommandMiddleware(object command, CommandContext context, Func<Task> next);

    public delegate Task<CommandContext> CommandDelegate(object command);

    public class CommandProcessorBuilder
    {
        private readonly List<ContextualCommandMiddleware> _middleware = new List<ContextualCommandMiddleware>();
        private readonly Dictionary<Type, Func<object, CommandContext, Task<object>>> _handlers = new Dictionary<Type, Func<object, CommandContext, Task<object>>>();

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

        public CommandProcessorBuilder Handle<TCommand>(Func<TCommand, Task> handler) =>
            Handle<TCommand>(async (c, _) =>
            {
                await handler(c);
                return null;
            });
        
        public CommandProcessorBuilder Handle<TCommand>(Func<TCommand, Task<object>> handler) =>
            Handle<TCommand>((c, _) => handler(c));

        public CommandProcessorBuilder Handle<TCommand>(Func<TCommand, CommandContext, Task> handler) =>
            Handle<TCommand>(async (c, ctx) =>
            {
                await handler(c, ctx);
                return null;
            });

        public CommandProcessorBuilder Handle<TCommand>(Func<TCommand, CommandContext, Task<object>> handler)
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
            var result = await handler(command, context);

            if (result != null)
            {
                context.WithResult(result);
            }
            
            context.Complete();
        }
    }
}