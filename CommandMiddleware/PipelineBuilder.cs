using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public delegate Task PipelineMiddleware(object input, Func<Task> next);
    public delegate Task PipelineMiddlewareWithContext(object input, PipelineContext context, Func<Task> next);
    public delegate Task<PipelineContext> PipelineDelegate(object input);

    public class PipelineBuilder
    {   
        private readonly List<PipelineMiddlewareWithContext> _middleware = new List<PipelineMiddlewareWithContext>();

        public PipelineBuilder Use(PipelineMiddleware middleware)
        {
            return Use((c, _, next) => middleware(c, next));
        }

        public PipelineBuilder Use(PipelineMiddlewareWithContext middleware)
        {
            _middleware.Add(middleware);
            return this;
        }
        
        public PipelineDelegate Build()
        {
            var pipeline = CreatePipeline(0);
            return input => pipeline(new PipelineContext(input));
        }
        
        private Func<PipelineContext, Task<PipelineContext>> CreatePipeline(int index)
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
}