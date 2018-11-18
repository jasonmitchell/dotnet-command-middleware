using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public delegate Task CommandMiddleware(object command, Func<Task> next);
    public delegate Task CommandHandler<in TCommand>(TCommand command);
    public delegate Task CommandProcessor(object command);

    public class CommandProcessorBuilder
    {
        private readonly List<CommandMiddleware> _middleware = new List<CommandMiddleware>();
        private readonly Dictionary<Type, Func<object, Task>> _handlers = new Dictionary<Type, Func<object, Task>>();

        public CommandProcessorBuilder()
        {
            Use(RequireHandler);
        }
        
        private async Task RequireHandler(object command, Func<Task> next)
        {
            if (!_handlers.ContainsKey(command.GetType()))
            {
                throw new InvalidOperationException($"Handler for {command.GetType().Name} not found");
            }

            await next();
        }

        public CommandProcessorBuilder Use(CommandMiddleware middleware)
        {
            _middleware.Add(middleware);
            return this;
        }

        public CommandProcessorBuilder Handle<TCommand>(CommandHandler<TCommand> handler)
        {
            _handlers.Add(typeof(TCommand), x => handler((TCommand) x));
            return this;
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

        public CommandProcessor Build()
        {
            var pipeline = _middleware.Any() ? CreatePipeline(0) : Execute;
            return c => pipeline(c);
        }
    }
}