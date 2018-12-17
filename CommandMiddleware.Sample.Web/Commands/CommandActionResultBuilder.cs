using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CommandMiddleware.Sample.Web.Commands
{
    public class CommandActionResultBuilder
    {
        private readonly Dictionary<Type, Func<object, ActionResult>> _handlers = new Dictionary<Type, Func<object, ActionResult>>();
        private readonly Task<CommandContext> _command;

        internal CommandActionResultBuilder(Task<CommandContext> command)
        {
            _command = command;

            Match<DomainError>(error => BadRequest(new[] {error}));
            Match<IEnumerable<DomainError>>(BadRequest);
        }

        public CommandActionResultBuilder Match<T>(Func<T, ActionResult> handler)
        {
            _handlers[typeof(T)] = x => handler((T)x);
            return this;
        }

        public async Task<ActionResult> ToActionResult()
        {
            var context = await _command;

            if (_handlers.TryGetValue(context.Response.GetType(), out var handler))
            {
                return handler(context.Response);
            }
            
            if (context.Response.Equals(Command.NoResponse))
            {
                return new NoContentResult();
            }
            
            return new OkObjectResult(context.Response);
        }
        
        private static ActionResult BadRequest(IEnumerable<DomainError> errors)
        {
            var modelState = new ModelStateDictionary();
            foreach (var error in errors)
            {
                modelState.AddModelError(error.Key, error.Message);
            }
            
            return new BadRequestObjectResult(modelState);
        }
    }
}