using System;
using System.Reflection;

namespace CommandMiddleware
{
    public class CommandHandlerNotFoundException : Exception
    {
        internal CommandHandlerNotFoundException(MemberInfo commandType)
            : base ($"No command handler found for {commandType.Name}.  Ensure the command handler has been registered") { }
    }
}