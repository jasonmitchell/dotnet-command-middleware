using System;
using System.Reflection;

namespace CommandMiddleware
{
    public class CommandHandlerAlreadyRegisteredException : Exception
    {
        internal CommandHandlerAlreadyRegisteredException(MemberInfo commandType)
            : base ($"A command handler for {commandType.Name} has already been registered.  Ensure only one handler for the command has been registered") { }
    }
}