using System;

namespace CommandMiddleware
{
    public class CommandContext : Pipeline.Context
    {
        public Type CommandType => Input.GetType();
        
        public bool RanToCompletion { get; private set; }
        public object Result { get; private set; }
        
        public CommandContext(object input) : base(input) {}

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