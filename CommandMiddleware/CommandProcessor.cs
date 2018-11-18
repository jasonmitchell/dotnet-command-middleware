using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandMiddleware
{
    public class CommandProcessor
    {
        private readonly Func<object, Task> _pipeline;
        private readonly IEnumerable<Type> _commandTypes;

        internal CommandProcessor(Func<object, Task> pipeline, IEnumerable<Type> commandTypes)
        {
            _pipeline = pipeline;
            _commandTypes = commandTypes;
        }
        
        public async Task Handle<TCommand>(TCommand command)
        {
            if (!_commandTypes.Contains(typeof(TCommand)))
            {
                throw new InvalidOperationException($"Handler for {command.GetType().Name} not found");
            }
            
            await _pipeline.Invoke(command);
        }
    }
}