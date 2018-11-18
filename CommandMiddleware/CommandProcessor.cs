using System;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public class CommandProcessor
    {
        private readonly Func<object, Task> _pipeline;

        internal CommandProcessor(Func<object, Task> pipeline)
        {
            _pipeline = pipeline;
        }
        
        public async Task Handle<TCommand>(TCommand command)
        {
            await _pipeline.Invoke(command);
        }
    }
}