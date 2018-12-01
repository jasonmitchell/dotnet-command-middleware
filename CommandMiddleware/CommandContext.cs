using System;

namespace CommandMiddleware
{
    public class CommandContext
    {
        public Type CommandType { get; }
        public object Response { get; private set; }
        
        internal CommandContext(Type commandType)
        {
            CommandType = commandType;
        }

        internal void WithResult(CommandResult result)
        {
            Response = result.State;
        }
    }
}