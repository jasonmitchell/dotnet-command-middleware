using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CommandMiddleware.Sample.Web.Commands
{
    public static class CommandContextExtensions
    {
        public static CommandActionResultBuilder Match<T>(this Task<CommandContext> command, Func<T, ActionResult> handler)
        {
            return new CommandActionResultBuilder(command).Match(handler);
        }
        
        public static Task<ActionResult> ToActionResult(this Task<CommandContext> command)
        {
            return new CommandActionResultBuilder(command).ToActionResult();
        }
    }
}