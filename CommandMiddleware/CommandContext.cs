using System;

namespace CommandMiddleware
{
    public class CommandContext
    {
        public Type CommandType { get; }
        
        public bool RanToCompletion { get; private set; }
        public object Result { get; private set; }

        public CommandContext(Type commandType)
        {
            CommandType = commandType;
        }

        public void WithResult(object result)
        {
            Result = result;
        }

        internal void Complete()
        {
            RanToCompletion = true;
        }
    }
}