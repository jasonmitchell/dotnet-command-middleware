using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public delegate Task<CommandContext> CommandDelegate(object input);
    
    public class CommandProcessor : Pipeline<CommandContext>
    {
        public delegate Task Handler<in TCommand>(TCommand command);
        public delegate Task HandlerWithContext<in TCommand>(TCommand command, CommandContext context);
        
        private readonly Dictionary<Type, Func<object, CommandContext, Task>> _handlers = new Dictionary<Type, Func<object, CommandContext, Task>>();

        public CommandProcessor()
        {
            Use(RequireHandler);
        }
        
        protected override CommandContext CreateContext(object command) => new CommandContext(command);
        
        public new CommandProcessor Use(Middleware middleware)
        {
            return Use((c, _, next) => middleware(c, next));
        }

        public new CommandProcessor Use(MiddlewareWithContext middleware)
        {
            base.Use(middleware);
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

        public new CommandDelegate Build()
        {
            Use(ExecuteCommand);

            var pipeline = base.Build();
            return x => pipeline(x);
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