using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public abstract class Pipeline<TContext> where TContext : Pipeline.Context
    {
        public delegate Task Middleware(object input, Func<Task> next);
        public delegate Task MiddlewareWithContext(object input, TContext context, Func<Task> next);
        public delegate Task<TContext> Delegate(object input);
        
        private readonly List<MiddlewareWithContext> _middleware = new List<MiddlewareWithContext>();

        protected abstract TContext CreateContext(object input);

        public Pipeline<TContext> Use(Middleware middleware)
        {
            return Use((c, _, next) => middleware(c, next));
        }

        public Pipeline<TContext> Use(MiddlewareWithContext middleware)
        {
            _middleware.Add(middleware);
            return this;
        }
        
        public Delegate Build()
        {
            var pipeline = CreatePipeline(0);
            return input => pipeline(CreateContext(input));
        }

        private Func<TContext, Task<TContext>> CreatePipeline(int index)
        {
            return async context =>
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
                    next = () => nextMiddleware(context);
                }

                var middleware = _middleware[index];
                await middleware(context.Input, context, next);

                return context;
            };
        }
    }
    
    public class Pipeline : Pipeline<Pipeline.Context>
    {
        protected override Context CreateContext(object input) => new Context(input);
        
        public class Context
        {
            public object Input { get; }

            public Context(object input)
            {
                Input = input;
            }
        }
    }
}