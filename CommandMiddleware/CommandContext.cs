using System;

namespace CommandMiddleware
{
    public class CommandContext
    {
        public Type CommandType { get; }
        public object Response { get; set; }
        
        public CommandContext(Type commandType)
        {
            CommandType = commandType;
        }
    }
}